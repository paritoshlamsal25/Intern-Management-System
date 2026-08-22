using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var attendance = _context.Attendances
                .Include(x => x.Intern)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                attendance = attendance.Where(x =>
                    x.Intern!.Name.Contains(search));
            }

            ViewBag.Search = search;

            return View(attendance
                .OrderByDescending(x => x.Date)
                .ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Interns = _context.Interns.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(Attendance attendance)
        {
            var alreadyMarked = _context.Attendances.Any(x =>
                x.InternId == attendance.InternId &&
                x.Date.Date == attendance.Date.Date);

            if (alreadyMarked)
            {
                ModelState.AddModelError(
                    "",
                    "Attendance has already been marked for this intern on this date.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Interns = _context.Interns.ToList();
                return View(attendance);
            }

            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            TempData["Success"] = "Attendance marked successfully.";

            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var attendance = _context.Attendances.Find(id);

            if (attendance == null)
            {
                return NotFound();
            }

            ViewBag.Interns = _context.Interns.ToList();

            return View(attendance);
        }

        [HttpPost]
        public IActionResult Edit(Attendance attendance)
        {
            var duplicate = _context.Attendances.Any(x =>
                x.Id != attendance.Id &&
                x.InternId == attendance.InternId &&
                x.Date.Date == attendance.Date.Date);

            if (duplicate)
            {
                ModelState.AddModelError(
                    "",
                    "Attendance has already been marked for this intern on this date.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Interns = _context.Interns.ToList();
                return View(attendance);
            }

            var existingAttendance = _context.Attendances.Find(attendance.Id);

            if (existingAttendance == null)
            {
                return NotFound();
            }

            existingAttendance.InternId = attendance.InternId;
            existingAttendance.Date = attendance.Date;
            existingAttendance.Status = attendance.Status;

            _context.SaveChanges();

            TempData["Success"] = "Attendance updated successfully.";

            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var attendance = _context.Attendances
                .Include(x => x.Intern)
                .FirstOrDefault(x => x.Id == id);

            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var attendance = _context.Attendances.Find(id);

            if (attendance == null)
            {
                return NotFound();
            }

            _context.Attendances.Remove(attendance);
            _context.SaveChanges();

            TempData["Success"] = "Attendance deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}