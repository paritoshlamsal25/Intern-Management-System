using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class InternController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InternController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var interns = _context.Interns
                        .Include(x => x.Department)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                interns = interns.Where(x =>
                    x.Name.Contains(search) ||
                    x.Email.Contains(search));
            }

            ViewBag.Search = search;

            return View(interns.ToList());
        }

        // GET
        public IActionResult Create()
        {
            ViewBag.Departments = _context.Departments.ToList();

            return View();
        }

        // POST
        [HttpPost]
         public IActionResult Create(Intern intern)
         {
             if (ModelState.IsValid)
             {
                 bool emailExists = _context.Interns.Any(x => x.Email == intern.Email);
                 if (emailExists)
                 {
                    ViewBag.Departments = _context.Departments.ToList();
                    ModelState.AddModelError("Email", "This email is already registered.");
                     return View(intern);
                 }
                 _context.Interns.Add(intern);
                 _context.SaveChanges();
                 TempData["Success"] = "Intern created successfully!";

                 return RedirectToAction("Index");
             }
            ViewBag.Departments = _context.Departments.ToList();

            return View(intern);
     }

        // GET
        public IActionResult Edit(int id)
        {
            var intern = _context.Interns.Find(id);

            if (intern == null)
            {
                return NotFound();
            }

            ViewBag.Departments = _context.Departments.ToList();

            return View(intern);
        }
        // POST
        [HttpPost]
        public IActionResult Edit(Intern intern)
        {
            if (ModelState.IsValid)
            {
                _context.Interns.Update(intern);
                _context.SaveChanges();
                TempData["Success"] = "Intern updated successfully!";

                return RedirectToAction("Index");
            }

            return View(intern);
        }
        public IActionResult Delete(int id)
        {
            var intern = _context.Interns
    .Include(x => x.Department)
    .FirstOrDefault(x => x.Id == id);
            return View(intern);
        }
        // POST
        [HttpPost]
        public IActionResult Delete(Intern intern)
        {
            var existingIntern = _context.Interns.Find(intern.Id);

            if (existingIntern != null)
            {
                _context.Interns.Remove(existingIntern);
                _context.SaveChanges();
                TempData["Success"] = "Intern deleted successfully!";
            }

          
            return RedirectToAction("Index");
        }
    }
}