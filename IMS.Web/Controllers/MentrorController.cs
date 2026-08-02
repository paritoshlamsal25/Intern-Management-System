using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    public class MentorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MentorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var mentors = _context.Mentors.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                mentors = mentors.Where(x =>
                    x.FullName.Contains(search) ||
                    x.Email.Contains(search));
            }

            ViewBag.Search = search;

            return View(mentors.ToList());
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Mentor mentor)
        {
            if (!ModelState.IsValid)
            {
                return View(mentor);
            }

            bool emailExists = _context.Mentors.Any(x => x.Email == mentor.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(mentor);
            }

            _context.Mentors.Add(mentor);
            _context.SaveChanges();

            TempData["Success"] = "Mentor created successfully.";

            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var mentor = _context.Mentors.Find(id);

            if (mentor == null)
            {
                return NotFound();
            }

            return View(mentor);
        }

        [HttpPost]
        public IActionResult Edit(Mentor mentor)
        {
            if (!ModelState.IsValid)
            {
                return View(mentor);
            }

            bool emailExists = _context.Mentors.Any(x =>
                x.Email == mentor.Email &&
                x.Id != mentor.Id);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(mentor);
            }

            _context.Mentors.Update(mentor);
            _context.SaveChanges();

            TempData["Success"] = "Mentor updated successfully.";

            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var mentor = _context.Mentors.Find(id);

            if (mentor == null)
            {
                return NotFound();
            }

            return View(mentor);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var mentor = _context.Mentors.Find(id);

            if (mentor == null)
            {
                return NotFound();
            }

            _context.Mentors.Remove(mentor);
            _context.SaveChanges();

            TempData["Success"] = "Mentor deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}