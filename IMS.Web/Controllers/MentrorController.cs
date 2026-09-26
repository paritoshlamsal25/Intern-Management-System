using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    [Authorize(Roles = "Admin,HR")] // accessible by both Admin and HR roles
    public class MentorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MentorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Mentors List
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var mentorsQuery = _context.Mentors.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                mentorsQuery = mentorsQuery.Where(x =>
                    (x.FullName != null && x.FullName.Contains(search)) ||
                    (x.Email != null && x.Email.Contains(search)));
            }

            ViewBag.Search = search;

            var result = await mentorsQuery.ToListAsync();
            return View(result);
        }

        // GET: Create Mentor
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Mentor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Mentor mentor)
        {
            if (!ModelState.IsValid)
            {
                return View(mentor);
            }

            bool emailExists = await _context.Mentors.AnyAsync(x => x.Email == mentor.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(mentor);
            }

            _context.Mentors.Add(mentor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Mentor created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit Mentor
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var mentor = await _context.Mentors.FindAsync(id);

            if (mentor == null)
            {
                return NotFound();
            }

            return View(mentor);
        }

        // POST: Edit Mentor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Mentor mentor)
        {
            if (!ModelState.IsValid)
            {
                return View(mentor);
            }

            bool emailExists = await _context.Mentors.AnyAsync(x =>
                x.Email == mentor.Email &&
                x.Id != mentor.Id);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(mentor);
            }

            _context.Mentors.Update(mentor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Mentor updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Delete Mentor Confirmation
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var mentor = await _context.Mentors.FindAsync(id);

            if (mentor == null)
            {
                return NotFound();
            }

            return View(mentor);
        }

        // POST: Delete Mentor
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mentor = await _context.Mentors.FindAsync(id);

            if (mentor == null)
            {
                return NotFound();
            }

            // Prevent deletion if interns are currently assigned to this mentor
            bool hasAssignedInterns = await _context.Interns.AnyAsync(i => i.MentorId == id);
            if (hasAssignedInterns)
            {
                TempData["Error"] = "Cannot delete this mentor because they are assigned to active interns.";
                return RedirectToAction(nameof(Index));
            }

            _context.Mentors.Remove(mentor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Mentor deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}