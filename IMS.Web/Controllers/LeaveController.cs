using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var leaves = _context.LeaveRequests
                .Include(x => x.Intern)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                leaves = leaves.Where(x =>
                    x.Intern!.Name.Contains(search));
            }

            ViewBag.Search = search;

            return View(leaves
                .OrderByDescending(x => x.Id)
                .ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Interns = _context.Interns.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(LeaveRequest leave)
        {
            if (leave.EndDate < leave.StartDate)
            {
                ModelState.AddModelError(
                    "",
                    "End date cannot be earlier than start date.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Interns = _context.Interns.ToList();
                return View(leave);
            }

            _context.LeaveRequests.Add(leave);
            _context.SaveChanges();

            TempData["Success"] = "Leave request created successfully.";

            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var leave = _context.LeaveRequests.Find(id);

            if (leave == null)
            {
                return NotFound();
            }

            ViewBag.Interns = _context.Interns.ToList();

            return View(leave);
        }

        [HttpPost]
        public IActionResult Edit(LeaveRequest leave)
        {
            if (leave.EndDate < leave.StartDate)
            {
                ModelState.AddModelError(
                    "",
                    "End date cannot be earlier than start date.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Interns = _context.Interns.ToList();
                return View(leave);
            }

            var existingLeave = _context.LeaveRequests.Find(leave.Id);

            if (existingLeave == null)
            {
                return NotFound();
            }

            existingLeave.InternId = leave.InternId;
            existingLeave.StartDate = leave.StartDate;
            existingLeave.EndDate = leave.EndDate;
            existingLeave.Reason = leave.Reason;

            _context.SaveChanges();

            TempData["Success"] = "Leave request updated successfully.";

            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var leave = _context.LeaveRequests
                .Include(x => x.Intern)
                .FirstOrDefault(x => x.Id == id);

            if (leave == null)
            {
                return NotFound();
            }

            return View(leave);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var leave = _context.LeaveRequests.Find(id);

            if (leave == null)
            {
                return NotFound();
            }

            _context.LeaveRequests.Remove(leave);
            _context.SaveChanges();

            TempData["Success"] = "Leave request deleted successfully.";

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var leave = _context.LeaveRequests.Find(id);

            if (leave == null)
            {
                return NotFound();
            }

            leave.Status = "Approved";

            _context.SaveChanges();

            TempData["Success"] = "Leave request approved.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            var leave = _context.LeaveRequests.Find(id);

            if (leave == null)
            {
                return NotFound();
            }

            leave.Status = "Rejected";

            _context.SaveChanges();

            TempData["Success"] = "Leave request rejected.";

            return RedirectToAction("Index");
        }
    }
}