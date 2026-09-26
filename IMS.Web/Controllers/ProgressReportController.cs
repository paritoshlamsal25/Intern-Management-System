using IMS.Web.Data;
using IMS.Web.Models;
using IMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Web.Controllers
{
    [Authorize] // Enforce authentication across all progress report endpoints
    public class ProgressReportController : Controller
    {
        private readonly IProgressReportService _service;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProgressReportController> _logger;

        public ProgressReportController(
            IProgressReportService service,
            ApplicationDbContext db,
            ILogger<ProgressReportController> logger)
        {
            _service = service;
            _db = db;
            _logger = logger;
        }

        private async Task<int?> GetCurrentInternIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;

            var intern = await _db.Interns.FirstOrDefaultAsync(i => i.Email == email);
            return intern?.Id;
        }

        // Helper: Get Current Mentor's Database ID
        private async Task<int?> GetCurrentMentorIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;

            var mentor = await _db.Mentors.FirstOrDefaultAsync(m => m.Email == email);
            return mentor?.Id;
        }

        // GET: Progress Reports List
        [HttpGet]
        public async Task<IActionResult> Index(int? internId, DateTime? from, DateTime? to)
        {
            try
            {
                // If current user is an intern, restrict view strictly to their own ID
                if (User.IsInRole("Intern"))
                {
                    var myId = await GetCurrentInternIdAsync();
                    if (!myId.HasValue)
                    {
                        TempData["Error"] = "No intern profile found for your account.";
                        return View(Enumerable.Empty<ProgressReport>());
                    }
                    internId = myId.Value;
                }
                // Mentor: restrict to assigned interns; also populate Interns dropdown filtered
                else if (User.IsInRole("Mentor"))
                {
                    var mentorId = await GetCurrentMentorIdAsync();
                    if (!mentorId.HasValue)
                        return View(Enumerable.Empty<ProgressReport>());

                    var myInterns = await _db.Interns.Where(i => i.MentorId == mentorId.Value).ToListAsync();
                    ViewBag.Interns = myInterns;

                    // If no internId filter passed, default to all of mentor's interns
                    if (!internId.HasValue)
                    {
                        var myInternIds = myInterns.Select(i => i.Id).ToList();
                        var items2 = await _db.ProgressReports
                            .Include(r => r.Intern)
                            .Include(r => r.Task)
                            .Where(r => myInternIds.Contains(r.InternId))
                            .OrderByDescending(r => r.Date)
                            .ToListAsync();
                        return View(items2);
                    }
                    else
                    {
                        // Verify the requested intern belongs to this mentor
                        var belongsToMe = myInterns.Any(i => i.Id == internId.Value);
                        if (!belongsToMe)
                            return Forbid();
                    }
                }
                else
                {
                    ViewBag.Interns = await _db.Interns.ToListAsync();
                }

                var items = await _db.ProgressReports
                    .Include(r => r.Intern)
                    .Include(r => r.Task)
                    .Where(r => !internId.HasValue || r.InternId == internId.Value)
                    .Where(r => !from.HasValue || r.Date >= from.Value)
                    .Where(r => !to.HasValue || r.Date <= to.Value)
                    .OrderByDescending(r => r.Date)
                    .ToListAsync();
                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading progress reports (internId={InternId}, from={From}, to={To})", internId, from, to);
                TempData["Error"] = "An error occurred while loading progress reports. Please try again or contact the administrator.";
                return View(Enumerable.Empty<ProgressReport>());
            }
        }

        // GET: Create Progress Report (Mentor/Admin/HR only)
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                var interns = mentorId.HasValue
                    ? await _db.Interns.Where(i => i.MentorId == mentorId.Value).ToListAsync()
                    : new List<Intern>();
                ViewBag.Interns = interns;

                // Pre-load tasks for first intern (if any) for JS to handle
                if (interns.Count == 1)
                {
                    ViewBag.Tasks = await _db.TaskItems
                        .Where(t => t.InternId == interns[0].Id && (t.Status == "Submitted" || t.Status == "Completed"))
                        .ToListAsync();
                    ViewBag.AllTasks = await _db.TaskItems
                        .Where(t => t.InternId == interns[0].Id)
                        .ToListAsync();
                }
            }
            else
            {
                ViewBag.Interns = await _db.Interns.ToListAsync();
            }

            return View();
        }

        // AJAX: Get tasks for an intern
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> GetInternTasks(int internId)
        {
            // If Mentor: verify the intern belongs to them
            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                var belongs = mentorId.HasValue && await _db.Interns.AnyAsync(i => i.Id == internId && i.MentorId == mentorId.Value);
                if (!belongs) return Forbid();
            }

            var tasks = await _db.TaskItems
                .Where(t => t.InternId == internId)
                .Select(t => new {
                    t.Id,
                    t.Title,
                    t.Status,
                    isSubmitted = t.Status == "Submitted" || t.Status == "Completed"
                })
                .ToListAsync();

            return Json(tasks);
        }

        // POST: Create Progress Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Create(ProgressReport report)
        {
            // Verify intern ownership for Mentor
            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                if (!mentorId.HasValue) return Forbid();
                var belongsToMe = await _db.Interns.AnyAsync(i => i.Id == report.InternId && i.MentorId == mentorId.Value);
                if (!belongsToMe)
                    ModelState.AddModelError("", "You can only create reports for your assigned interns.");

                // If a task is linked, verify it also belongs to the intern
                if (report.TaskId.HasValue)
                {
                    var taskOk = await _db.TaskItems.AnyAsync(t => t.Id == report.TaskId.Value && t.InternId == report.InternId);
                    if (!taskOk)
                        ModelState.AddModelError("", "The selected task does not belong to this intern.");
                }
            }

            // Always default status to Completed
            report.Status = "Completed";

            // Remove Status from ModelState (it's not in the form)
            ModelState.Remove(nameof(report.Status));

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Mentor"))
                {
                    var mentorId = await GetCurrentMentorIdAsync();
                    ViewBag.Interns = mentorId.HasValue
                        ? await _db.Interns.Where(i => i.MentorId == mentorId.Value).ToListAsync()
                        : new List<Intern>();
                }
                else
                    ViewBag.Interns = await _db.Interns.ToListAsync();

                return View(report);
            }

            await _service.CreateAsync(report);
            TempData["Success"] = "Progress report added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Export Progress Reports CSV
        [HttpGet]
        public async Task<IActionResult> ExportCsv(int? internId, DateTime? from, DateTime? to)
        {
            // Enforce ownership if intern
            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (!myId.HasValue) return Forbid();
                internId = myId.Value;
            }
            else if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                if (!mentorId.HasValue) return Forbid();
                if (internId.HasValue)
                {
                    var belongsToMe = await _db.Interns.AnyAsync(i => i.Id == internId.Value && i.MentorId == mentorId.Value);
                    if (!belongsToMe) return Forbid();
                }
            }

            var items = await _service.GetByFilterAsync(internId, from, to);

            var csv = new StringBuilder();
            csv.AppendLine("Id,Intern,Date,Status,Notes");

            foreach (var r in items)
            {
                var internName = CleanCsvField(r.Intern?.Name);
                var notes = CleanCsvField(r.Notes);
                var status = CleanCsvField(r.Status);

                csv.AppendLine($"{r.Id},{internName},{r.Date:yyyy-MM-dd},{status},{notes}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"progress-reports-{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Print Progress Reports View
        [HttpGet]
        public async Task<IActionResult> Print(int? internId, DateTime? from, DateTime? to)
        {
            // Enforce ownership if intern
            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (!myId.HasValue) return Forbid();
                internId = myId.Value;
            }

            var items = await _service.GetByFilterAsync(internId, from, to);
            return View("Print", items);
        }

        private static string CleanCsvField(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "\"\"";

            // Escape double quotes by doubling them and enclose in quotes
            return $"\"{input.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ")}\"";
        }

        // ─────────────────────────────────────────────────
        // GET: Edit Progress Report
        // ─────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Edit(int id)
        {
            var report = await _db.ProgressReports
                .Include(r => r.Intern)
                .Include(r => r.Task)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();

            // Mentor can only edit reports for their own interns
            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                var belongs = mentorId.HasValue && await _db.Interns.AnyAsync(i => i.Id == report.InternId && i.MentorId == mentorId.Value);
                if (!belongs) return Forbid();
                ViewBag.Interns = await _db.Interns.Where(i => i.MentorId == mentorId.Value).ToListAsync();
            }
            else
            {
                ViewBag.Interns = await _db.Interns.ToListAsync();
            }

            ViewBag.Tasks = await _db.TaskItems
                .Where(t => t.InternId == report.InternId)
                .ToListAsync();

            return View(report);
        }

        // POST: Edit Progress Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Edit(int id, ProgressReport report)
        {
            if (id != report.Id) return BadRequest();

            var existing = await _db.ProgressReports.FindAsync(id);
            if (existing == null) return NotFound();

            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                var belongs = mentorId.HasValue && await _db.Interns.AnyAsync(i => i.Id == existing.InternId && i.MentorId == mentorId.Value);
                if (!belongs) return Forbid();
            }

            ModelState.Remove(nameof(report.Status));

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Mentor"))
                {
                    var mentorId = await GetCurrentMentorIdAsync();
                    ViewBag.Interns = mentorId.HasValue
                        ? await _db.Interns.Where(i => i.MentorId == mentorId.Value).ToListAsync()
                        : new List<Intern>();
                }
                else
                    ViewBag.Interns = await _db.Interns.ToListAsync();

                ViewBag.Tasks = await _db.TaskItems.Where(t => t.InternId == report.InternId).ToListAsync();
                return View(report);
            }

            existing.Date = report.Date;
            existing.Topic = report.Topic;
            existing.Marks = report.Marks;
            existing.MaxMarks = report.MaxMarks;
            existing.Notes = report.Notes;
            existing.AssignmentReference = report.AssignmentReference;
            existing.TaskId = report.TaskId;
            existing.Status = "Completed";

            await _db.SaveChangesAsync();
            TempData["Success"] = "Progress report updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete Confirm
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _db.ProgressReports
                .Include(r => r.Intern)
                .Include(r => r.Task)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();

            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                var belongs = mentorId.HasValue && await _db.Interns.AnyAsync(i => i.Id == report.InternId && i.MentorId == mentorId.Value);
                if (!belongs) return Forbid();
            }

            return View(report);
        }

        // POST: Delete Progress Report
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _db.ProgressReports.FindAsync(id);
            if (report == null) return NotFound();

            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                var belongs = mentorId.HasValue && await _db.Interns.AnyAsync(i => i.Id == report.InternId && i.MentorId == mentorId.Value);
                if (!belongs) return Forbid();
            }

            _db.ProgressReports.Remove(report);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Progress report deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}