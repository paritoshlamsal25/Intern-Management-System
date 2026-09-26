using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    [Authorize(Roles = "Admin,HR")] // Accessible by both Admin and HR roles
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments List
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var departments = _context.Departments.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                departments = departments.Where(x => x.Name != null && x.Name.Contains(search));
            }

            ViewBag.Search = search;

            var result = await departments.ToListAsync();
            return View(result);
        }

        // GET: Create Department
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Department
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.Departments.AnyAsync(x => x.Name == department.Name);

                if (exists)
                {
                    ModelState.AddModelError("Name", "Department name already exists.");
                    return View(department);
                }

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Department created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // GET: Edit Department
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Edit Department
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.Departments.AnyAsync(x =>
                    x.Name == department.Name &&
                    x.Id != department.Id);

                if (exists)
                {
                    ModelState.AddModelError("Name", "Department name already exists.");
                    return View(department);
                }

                _context.Departments.Update(department);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Department updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // GET: Delete Department Confirmation
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Delete Department
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department != null)
            {
                // Prevent deletion if interns are currently assigned to this department
                bool hasAssignedInterns = await _context.Interns.AnyAsync(i => i.DepartmentId == id);
                if (hasAssignedInterns)
                {
                    TempData["Error"] = "Cannot delete this department because there are interns assigned to it.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Department deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}