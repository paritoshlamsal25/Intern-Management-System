using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IMS.Web.Controllers
{
    [Authorize] // Enforce authentication across all endpoints
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        // GET: Leave Requests
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var leavesQuery = _context.LeaveRequests
                .Include(x => x.Intern)
                .AsQueryable();

            // Intern: see only own requests
            if (User.IsInRole("Intern"))
            {
                var myId = await GetCurrentInternIdAsync();
                if (myId.HasValue)
                    leavesQuery = leavesQuery.Where(x => x.InternId == myId.Value);
                else
                    return View(Enumerable.Empty<LeaveRequest>());
            }
            // Mentor: see only requests of their assigned interns
            else if (User.IsInRole("Mentor"))
            {
                var mentorId = await GetCurrentMentorIdAsync();
                if (mentorId.HasValue)
                    leavesQuery = leavesQuery.Where(x => x.Intern != null && x.Intern.MentorId == mentorId.Value);
                else
                    return View(Enumerable.Empty<LeaveRequest>());
            }

            if (!string.IsNullOrEmpty(search))
            {
                leavesQuery = leavesQuery.Where(x =>
                    x.Intern != null && x.Intern.Name.Contains(search));
            }

            ViewBag.Search = search;

            var result = await leavesQuery
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(result);
        }

        // GET: Create Leave Request (Interns only)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!User.IsInRole("Intern"))
                return Forbid();

            var myId = await GetCurrentInternIdAsync();
            if (myId.HasValue)
            {
                ViewBag.CurrentInternId = myId.Value;
                ViewBag.CurrentInternName = await _context.Interns
                    .Where(i => i.Id == myId.Value)
                    .Select(i => i.Name)
                    .FirstOrDefaultAsync();
            }

            return View(new LeaveRequest());
        }

        // POST: Create Leave Request (Interns only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequest leave)
        {
            if (!User.IsInRole("Intern"))
                return Forbid();

            var myId = await GetCurrentInternIdAsync();
            if (myId.HasValue)
            {
                leave.InternId = myId.Value;
                ViewBag.CurrentInternId = myId.Value;
                ViewBag.CurrentInternName = await _context.Interns
                    .Where(i => i.Id == myId.Value)
                    .Select(i => i.Name)
                    .FirstOrDefaultAsync();
            }

            if (leave.EndDate < leave.StartDate)
                ModelState.AddModelError("", "End date cannot be earlier than start date.");

            if (!ModelState.IsValid)
            {
                ViewBag.Interns = await _context.Interns.ToListAsync();
                return View(leave);
            }

            leave.Status ??= "Pending";

            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit Leave Request (Interns only)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.IsInRole("Intern"))
                return Forbid();

            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound();

            var myId = await GetCurrentInternIdAsync();
            if (!myId.HasValue || leave.InternId != myId.Value)
                return Forbid();

            if (leave.Status != "Pending")
            {
                TempData["Error"] = "Processed leave requests cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Interns = await _context.Interns.ToListAsync();
            return View(leave);
        }

        // POST: Edit Leave Request (Interns only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LeaveRequest leave)
        {
            if (!User.IsInRole("Intern"))
                return Forbid();

            var existingLeave = await _context.LeaveRequests.FindAsync(leave.Id);
            if (existingLeave == null) return NotFound();

            var myId = await GetCurrentInternIdAsync();
            if (!myId.HasValue || existingLeave.InternId != myId.Value)
                return Forbid();

            if (existingLeave.Status != "Pending")
            {
                TempData["Error"] = "Processed leave requests cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            leave.InternId = myId.Value;

            if (leave.EndDate < leave.StartDate)
                ModelState.AddModelError("", "End date cannot be earlier than start date.");

            if (!ModelState.IsValid)
            {
                ViewBag.Interns = await _context.Interns.ToListAsync();
                return View(leave);
            }

            existingLeave.InternId = leave.InternId;
            existingLeave.StartDate = leave.StartDate;
            existingLeave.EndDate = leave.EndDate;
            existingLeave.Reason = leave.Reason;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete Confirmation (Interns only)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Intern"))
                return Forbid();

            var leave = await _context.LeaveRequests
                .Include(x => x.Intern)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (leave == null) return NotFound();

            var myId = await GetCurrentInternIdAsync();
            if (!myId.HasValue || leave.InternId != myId.Value)
                return Forbid();

            return View(leave);
        }

        // POST: Delete Confirmation (Interns only)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole("Intern"))
                return Forbid();

            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound();

            var myId = await GetCurrentInternIdAsync();
            if (!myId.HasValue || leave.InternId != myId.Value)
                return Forbid();

            _context.LeaveRequests.Remove(leave);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Approve Leave Request (Admin & HR Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Approve(int id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);

            if (leave == null)
            {
                return NotFound();
            }

            leave.Status = "Approved";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request approved.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Reject Leave Request (Admin & HR Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Reject(int id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);

            if (leave == null)
            {
                return NotFound();
            }

            leave.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Leave request rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}