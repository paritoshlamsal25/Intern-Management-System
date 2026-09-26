using IMS.Web.Data;
using IMS.Web.Models;
using IMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IMS.Web.Controllers
{
    [Authorize] // Requires authentication for all endpoints
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(ApplicationDbContext context, IAuditService auditService, ILogger<AttendanceController> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        // Helper: Get Current Intern's Database ID
        private async Task<int?> GetCurrentInternIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;

            var intern = await _context.Interns.FirstOrDefaultAsync(i => i.Email == email);
            return intern?.Id;
        }

        // Helper: Get Current Mentor's Database ID
        private async Task<int?> GetCurrentMentorIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;

            var mentor = await _context.Mentors.FirstOrDefaultAsync(m => m.Email == email);
            return mentor?.Id;
        }

        // Helper: Get Current Intern's Full Name
        private async Task<string?> GetCurrentInternNameAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;

            var intern = await _context.Interns.FirstOrDefaultAsync(i => i.Email == email);
            return intern?.Name;
        }

        // GET: Attendance List (Filtered by Role and Search)
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var attendanceQuery = _context.Attendances
                .Include(x => x.Intern)
                .AsQueryable();

            // Intern: only see own attendance
            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (myId.HasValue)
                    attendanceQuery = attendanceQuery.Where(x => x.InternId == myId.Value);
                else
                    return View(Enumerable.Empty<Attendance>());
            }
            // Mentor: only see attendance of their assigned interns
            else if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                if (mentorId.HasValue)
                    attendanceQuery = attendanceQuery.Where(x => x.Intern != null && x.Intern.MentorId == mentorId.Value);
                else
                    return View(Enumerable.Empty<Attendance>()); // No mentor record found → show nothing
            }

            if (!string.IsNullOrEmpty(search))
            {
                attendanceQuery = attendanceQuery.Where(x =>
                    x.Intern != null && x.Intern.Name.Contains(search));
            }

            ViewBag.Search = search;

            var result = await attendanceQuery
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return View(result);
        }

        // GET: Mark Attendance
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Intern")]
        public async Task<IActionResult> Create()
        {
            // If intern, pass their details to hide the dropdown selection in the View
            if (User.IsInRole("Intern"))
            {
                ViewBag.CurrentInternId = await GetCurrentInternIdAsync();
                ViewBag.CurrentInternName = await GetCurrentInternNameAsync();
                return View();
            }

            ViewBag.Interns = await _context.Interns.ToListAsync();
            return View();
        }

        // POST: Mark Attendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR,Intern")]
        public async Task<IActionResult> Create(Attendance attendance)
        {
            // If current user is intern, enforce ownership and override InternId
            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (!myId.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "No intern profile found for your account.");
                }
                else
                {
                    attendance.InternId = myId.Value;
                }
            }

            // Check for duplicate attendance on the same date
            var alreadyMarked = await _context.Attendances.AnyAsync(x =>
                x.InternId == attendance.InternId &&
                x.Date.Date == attendance.Date.Date);

            if (alreadyMarked)
            {
                ModelState.AddModelError(string.Empty, "Attendance has already been marked for this intern on this date.");
            }

            if (!ModelState.IsValid)
            {
                if (!User.IsInRole("Intern"))
                {
                    ViewBag.Interns = await _context.Interns.ToListAsync();
                }
                else
                {
                    ViewBag.CurrentInternId = await GetCurrentInternIdAsync();
                    ViewBag.CurrentInternName = await GetCurrentInternNameAsync();
                }

                return View(attendance);
            }

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Create", "Attendance", attendance.Id, User?.Identity?.Name ?? "system");
            _logger.LogInformation("Attendance created: {AttendanceId} for Intern {InternId}", attendance.Id, attendance.InternId);

            TempData["Success"] = "Attendance marked successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit Attendance (Admin & HR Only)
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);

            if (attendance == null)
            {
                return NotFound();
            }

            ViewBag.Interns = await _context.Interns.ToListAsync();
            return View(attendance);
        }

        // POST: Edit Attendance (Admin & HR Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(Attendance attendance)
        {
            var duplicate = await _context.Attendances.AnyAsync(x =>
                x.Id != attendance.Id &&
                x.InternId == attendance.InternId &&
                x.Date.Date == attendance.Date.Date);

            if (duplicate)
            {
                ModelState.AddModelError(string.Empty, "Attendance has already been marked for this intern on this date.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Interns = await _context.Interns.ToListAsync();
                return View(attendance);
            }

            var existingAttendance = await _context.Attendances.FindAsync(attendance.Id);

            if (existingAttendance == null)
            {
                return NotFound();
            }

            existingAttendance.InternId = attendance.InternId;
            existingAttendance.Date = attendance.Date;
            existingAttendance.Status = attendance.Status;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Update", "Attendance", attendance.Id, User?.Identity?.Name ?? "system");
            _logger.LogInformation("Attendance updated: {AttendanceId} by {User}", attendance.Id, User?.Identity?.Name ?? "system");

            TempData["Success"] = "Attendance updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete Attendance Confirmation (Admin & HR Only)
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Delete(int id)
        {
            var attendance = await _context.Attendances
                .Include(x => x.Intern)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // POST: Delete Attendance (Admin & HR Only)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);

            if (attendance == null)
            {
                return NotFound();
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Delete", "Attendance", id, User?.Identity?.Name ?? "system");
            _logger.LogInformation("Attendance deleted: {AttendanceId} by {User}", id, User?.Identity?.Name ?? "system");

            TempData["Success"] = "Attendance deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}