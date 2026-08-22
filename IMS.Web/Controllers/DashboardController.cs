using IMS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalInterns = _context.Interns.Count();
            ViewBag.TotalDepartments = _context.Departments.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalTasks = _context.TaskItems.Count();

            ViewBag.PendingTasks = _context.TaskItems
                .Count(x => x.Status == "Pending");

            ViewBag.CompletedTasks = _context.TaskItems
                .Count(x => x.Status == "Completed");
            ViewBag.TotalAttendance = _context.Attendances.Count();

            ViewBag.PresentCount = _context.Attendances
                .Count(x => x.Status == "Present");

            ViewBag.AbsentCount = _context.Attendances
                .Count(x => x.Status == "Absent");

            ViewBag.LateCount = _context.Attendances
                .Count(x => x.Status == "Late");

            var recentInterns = _context.Interns
                .Include(x => x.Department)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToList();

            return View(recentInterns);
        }
    }
}