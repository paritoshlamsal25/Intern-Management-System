using IMS.Web.Data;
using IMS.Web.Models;
using IMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IMS.Web.Controllers
{
    [Authorize] // Requires authentication across all endpoints
    public class InternController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasherService _hasher;

        public InternController(ApplicationDbContext context, IPasswordHasherService hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // GET: Interns List
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var internsQuery = _context.Interns
                .Include(x => x.Department)
                .Include(x => x.Mentor)
                .AsQueryable();

            // If current user is an intern, restrict view to only their own profile
            if (User.IsInRole("Intern"))
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    internsQuery = internsQuery.Where(x => x.Email == email);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                internsQuery = internsQuery.Where(x =>
                    (x.Name != null && x.Name.Contains(search)) ||
                    (x.Email != null && x.Email.Contains(search)));
            }

            ViewBag.Search = search;

            var result = await internsQuery.ToListAsync();
            return View(result);
        }

        // GET: Create Intern (Admin & HR Only)
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Mentors = await _context.Mentors.ToListAsync();

            return View();
        }

        // POST: Create Intern (Admin & HR Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create(Intern intern, string Password, string ConfirmPassword)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }

            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            }

            if (ModelState.IsValid)
            {
                // Check existing user or intern email
                bool userExists = await _context.Users.AnyAsync(x => x.Email == intern.Email);
                if (userExists)
                {
                    ViewBag.Departments = await _context.Departments.ToListAsync();
                    ViewBag.Mentors = await _context.Mentors.ToListAsync();
                    ModelState.AddModelError("Email", "This email is already registered as a user.");
                    return View(intern);
                }

                bool internEmailExists = await _context.Interns.AnyAsync(x => x.Email == intern.Email);
                if (internEmailExists)
                {
                    ViewBag.Departments = await _context.Departments.ToListAsync();
                    ViewBag.Mentors = await _context.Mentors.ToListAsync();
                    ModelState.AddModelError("Email", "This email is already registered as an intern.");
                    return View(intern);
                }

                // Create user login account for the intern
                var user = new User
                {
                    FullName = intern.Name,
                    Email = intern.Email,
                    Role = "Intern"
                };

                user.Password = _hasher != null ? _hasher.HashPassword(user, Password) : Password;

                _context.Users.Add(user);
                _context.Interns.Add(intern);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Intern created successfully! Credentials assigned.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Mentors = await _context.Mentors.ToListAsync();

            return View(intern);
        }

        // GET: Edit Intern (Admin & HR Only)
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(int id)
        {
            var intern = await _context.Interns.FindAsync(id);

            if (intern == null)
            {
                return NotFound();
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Mentors = await _context.Mentors.ToListAsync();

            return View(intern);
        }

        // POST: Edit Intern (Admin & HR Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(Intern intern)
        {
            if (ModelState.IsValid)
            {
                var existingIntern = await _context.Interns.AsNoTracking().FirstOrDefaultAsync(i => i.Id == intern.Id);

                if (existingIntern != null)
                {
                    // Sync User table FullName if Name changed
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == existingIntern.Email);
                    if (user != null)
                    {
                        user.FullName = intern.Name;
                        user.Email = intern.Email;
                        _context.Users.Update(user);
                    }
                }

                _context.Interns.Update(intern);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Intern updated successfully!";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Mentors = await _context.Mentors.ToListAsync();

            return View(intern);
        }

        // GET: Delete Intern Confirmation (Admin & HR Only)
        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Delete(int id)
        {
            var intern = await _context.Interns
                .Include(x => x.Department)
                .Include(x => x.Mentor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (intern == null)
            {
                return NotFound();
            }

            return View(intern);
        }

        // POST: Delete Intern (Admin & HR Only)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var existingIntern = await _context.Interns.FindAsync(id);

            if (existingIntern != null)
            {
                // Delete associated user account to revoke login access
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == existingIntern.Email);
                if (user != null)
                {
                    _context.Users.Remove(user);
                }

                _context.Interns.Remove(existingIntern);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Intern and associated user account deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}