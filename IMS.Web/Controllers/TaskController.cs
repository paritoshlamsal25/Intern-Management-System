using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TaskController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private async Task<int?> GetCurrentInternIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;

            var intern = await _context.Interns.FirstOrDefaultAsync(i => i.Email == email);
            return intern?.Id;
        }

        private async Task<int?> GetCurrentMentorIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;

            var mentor = await _context.Mentors.FirstOrDefaultAsync(m => m.Email == email);
            return mentor?.Id;
        }

        // GET: Tasks List
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var tasksQuery = _context.TaskItems
                .Include(x => x.Intern)
                    .ThenInclude(i => i!.Mentor)
                .AsQueryable();

            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (myId.HasValue)
                {
                    tasksQuery = tasksQuery.Where(t => t.InternId == myId.Value);
                }
                else
                {
                    return View(Enumerable.Empty<TaskItem>());
                }
            }
            else if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                if (mentorId.HasValue)
                    tasksQuery = tasksQuery.Where(t => t.Intern != null && t.Intern.MentorId == mentorId.Value);
                else
                    return View(Enumerable.Empty<TaskItem>()); // No mentor record found → show nothing
            }

            if (!string.IsNullOrEmpty(search))
            {
                tasksQuery = tasksQuery.Where(x =>
                    (x.Title != null && x.Title.Contains(search)) ||
                    (x.Intern != null && x.Intern.Name != null && x.Intern.Name.Contains(search)));
            }

            ViewBag.Search = search;

            var result = await tasksQuery.OrderByDescending(t => t.Id).ToListAsync();
            return View(result);
        }

        // GET: Task Details & Submissions
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _context.TaskItems
                .Include(t => t.Intern)
                    .ThenInclude(i => i!.Department)
                .Include(t => t.Intern)
                    .ThenInclude(i => i!.Mentor)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            // Intern: can only view own tasks
            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (!myId.HasValue || task.InternId != myId.Value)
                    return Forbid();
            }
            // Mentor: can only view tasks belonging to their assigned interns
            else if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                if (!mentorId.HasValue || task.Intern == null || task.Intern.MentorId != mentorId.Value)
                    return Forbid();
            }

            return View(task);
        }

        // GET: Create Task (Admin, HR & Mentor)
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                ViewBag.Interns = await _context.Interns
                    .Where(i => i.MentorId == mentorId)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Interns = await _context.Interns.ToListAsync();
            }

            return View();
        }

        // POST: Create Task with File Upload (Admin, HR & Mentor)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Create(TaskItem task, IFormFile? attachmentFile)
        {
            // If Mentor: verify the selected intern is actually assigned to them
            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                var belongsToMe = mentorId.HasValue && await _context.Interns
                    .AnyAsync(i => i.Id == task.InternId && i.MentorId == mentorId.Value);

                if (!belongsToMe)
                {
                    ModelState.AddModelError("", "You can only assign tasks to your own interns.");
                    var myMentorId = await GetCurrentMentorIdAsync();
                    ViewBag.Interns = await _context.Interns
                        .Where(i => i.MentorId == myMentorId)
                        .ToListAsync();
                    return View(task);
                }
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Mentor"))
                {
                    var myMentorId = await GetCurrentMentorIdAsync();
                    ViewBag.Interns = await _context.Interns
                        .Where(i => i.MentorId == myMentorId)
                        .ToListAsync();
                }
                else
                {
                    ViewBag.Interns = await _context.Interns.ToListAsync();
                }
                return View(task);
            }

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "tasks");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(attachmentFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await attachmentFile.CopyToAsync(stream);

                task.AttachmentPath = $"/uploads/tasks/{safeFileName}";
                task.AttachmentFileName = attachmentFile.FileName;
            }

            task.Status ??= "Pending";
            task.CreatedDate = DateTime.Today;

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task assigned successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit Task (Admin, HR & Mentor)
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.TaskItems
                .Include(t => t.Intern)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                // Verify this task belongs to one of the mentor's interns
                if (!mentorId.HasValue || task.Intern == null || task.Intern.MentorId != mentorId.Value)
                    return Forbid();

                ViewBag.Interns = await _context.Interns
                    .Where(i => i.MentorId == mentorId.Value)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Interns = await _context.Interns.ToListAsync();
            }

            return View(task);
        }

        // POST: Edit Task (Admin, HR & Mentor)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Edit(TaskItem task, IFormFile? attachmentFile)
        {
            var existingTask = await _context.TaskItems
                .Include(t => t.Intern)
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            if (existingTask == null)
                return NotFound();

            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                // Verify original task belongs to mentor's intern
                if (!mentorId.HasValue || existingTask.Intern == null || existingTask.Intern.MentorId != mentorId.Value)
                    return Forbid();

                // Also verify the new intern selection (if changed) belongs to mentor
                var newInternBelongs = await _context.Interns
                    .AnyAsync(i => i.Id == task.InternId && i.MentorId == mentorId.Value);
                if (!newInternBelongs)
                {
                    ModelState.AddModelError("", "You can only assign tasks to your own interns.");
                    ViewBag.Interns = await _context.Interns
                        .Where(i => i.MentorId == mentorId.Value)
                        .ToListAsync();
                    return View(task);
                }
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Mentor"))
                {
                    var mentorId = await GetCurrentMentorIdAsync();
                    ViewBag.Interns = await _context.Interns
                        .Where(i => i.MentorId == mentorId)
                        .ToListAsync();
                }
                else
                {
                    ViewBag.Interns = await _context.Interns.ToListAsync();
                }
                return View(task);
            }

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "tasks");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(attachmentFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await attachmentFile.CopyToAsync(stream);

                existingTask.AttachmentPath = $"/uploads/tasks/{safeFileName}";
                existingTask.AttachmentFileName = attachmentFile.FileName;
            }

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.DueDate = task.DueDate;
            existingTask.Status = task.Status;
            existingTask.InternId = task.InternId;

            if (task.Status == "Completed" && !existingTask.CompletedDate.HasValue)
                existingTask.CompletedDate = DateTime.Today;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Submit Work & Progress by Intern (Notes + File Upload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWork(int id, string? submissionNote, string status, IFormFile? submissionFile)
        {
            var task = await _context.TaskItems.FindAsync(id);

            if (task == null)
                return NotFound();

            // Only interns assigned to this task may submit work
            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (!myId.HasValue || task.InternId != myId.Value)
                    return Forbid();

                // Intern can only mark as "In Progress" or "Submitted" — NEVER "Completed"
                // "Completed" can only be set by Mentor or Admin after reviewing the submission
                if (status == "Completed")
                    status = "Submitted";
            }

            if (submissionFile != null && submissionFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "submissions");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(submissionFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await submissionFile.CopyToAsync(stream);

                task.SubmissionFilePath = $"/uploads/submissions/{safeFileName}";
                task.SubmissionFileName = submissionFile.FileName;
            }

            task.SubmissionNote = submissionNote;
            task.Status = string.IsNullOrWhiteSpace(status) ? "In Progress" : status;
            task.SubmittedDate = DateTime.Now;

            // Only set CompletedDate if Mentor/Admin actually marks it Completed
            if (task.Status == "Completed" && !User.IsInRole("Intern"))
                task.CompletedDate = DateTime.Today;

            await _context.SaveChangesAsync();

            TempData["Success"] = User.IsInRole("Intern")
                ? "Your work has been submitted. Waiting for mentor review."
                : "Task submission updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Quick Status Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var task = await _context.TaskItems.FindAsync(id);

            if (task == null)
                return NotFound();

            // Interns cannot set status to Completed — only Mentor/Admin can approve
            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (!myId.HasValue || task.InternId != myId.Value)
                    return Forbid();

                if (status == "Completed")
                {
                    TempData["Error"] = "Only a Mentor or Admin can mark a task as Completed.";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            task.Status = status;
            if (status == "Completed")
                task.CompletedDate = DateTime.Today;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Task status updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete Task Confirmation (Admin, HR & Mentor)
        [HttpGet]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.TaskItems
                .Include(x => x.Intern)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (task == null)
                return NotFound();

            // Mentor: can only delete tasks for their own interns
            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                if (!mentorId.HasValue || task.Intern == null || task.Intern.MentorId != mentorId.Value)
                    return Forbid();
            }

            return View(task);
        }

        // POST: Delete Task (Admin, HR & Mentor)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR,Mentor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.TaskItems
                .Include(t => t.Intern)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            // Mentor: can only delete tasks for their own interns
            if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                if (!mentorId.HasValue || task.Intern == null || task.Intern.MentorId != mentorId.Value)
                    return Forbid();
            }

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}