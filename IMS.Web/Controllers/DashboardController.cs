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

            var recentInterns = _context.Interns
                .Include(x => x.Department)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToList();

            return View(recentInterns);
        }
    }
}