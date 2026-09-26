using IMS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    [Authorize(Policy = "AdminOnly")] // Strictly locked to Admins only
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AuditController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var q = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                q = q.Where(a =>
                    (a.Action != null && a.Action.Contains(search)) ||
                    (a.Entity != null && a.Entity.Contains(search)) ||
                    (a.PerformedBy != null && a.PerformedBy.Contains(search)));
            }

            ViewBag.Search = search;

            var logs = await q
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(logs);
        }
    }
}