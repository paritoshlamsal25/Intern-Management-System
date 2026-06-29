using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var departments = _context.Departments.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                departments = departments.Where(x => x.Name.Contains(search));
            }

            ViewBag.Search = search;

            return View(departments.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                bool exists = _context.Departments.Any(x => x.Name == department.Name);

                if (exists)
                {
                    ModelState.AddModelError("Name", "Department already exists.");
                    return View(department);
                }

                _context.Departments.Add(department);
                _context.SaveChanges();

                TempData["Success"] = "Department created successfully!";

                return RedirectToAction("Index");
            }

            return View(department);
        }

        public IActionResult Edit(int id)
        {
            var department = _context.Departments.Find(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpPost]
        public IActionResult Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                bool exists = _context.Departments.Any(x =>
                    x.Name == department.Name &&
                    x.Id != department.Id);

                if (exists)
                {
                    ModelState.AddModelError("Name", "Department already exists.");
                    return View(department);
                }

                _context.Departments.Update(department);
                _context.SaveChanges();

                TempData["Success"] = "Department updated successfully!";

                return RedirectToAction("Index");
            }

            return View(department);
        }

        public IActionResult Delete(int id)
        {
            var department = _context.Departments.Find(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var department = _context.Departments.Find(id);

            if (department != null)
            {
                _context.Departments.Remove(department);
                _context.SaveChanges();

                TempData["Success"] = "Department deleted successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}