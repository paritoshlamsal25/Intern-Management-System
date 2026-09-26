using IMS.Web.Data;
using IMS.Web.Models;
using IMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IMS.Web.Controllers
{
    [Authorize(Policy = "AdminOnly")] // Strictly locked to Admins
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasherService _hasher;

        public UserController(ApplicationDbContext context, IPasswordHasherService hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        private static List<string> GetAvailableRoles() => new() { "Admin", "HR", "Mentor", "Intern" };

        // GET: Users List
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(x =>
                    (x.FullName != null && x.FullName.Contains(search)) ||
                    (x.Email != null && x.Email.Contains(search)) ||
                    (x.Role != null && x.Role.Contains(search)));
            }

            ViewBag.Search = search;

            var result = await usersQuery.ToListAsync();
            return View(result);
        }

        // GET: Create User
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = GetAvailableRoles();
            return View();
        }

        // POST: Create User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                bool emailExists = await _context.Users.AnyAsync(x => x.Email == user.Email);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    ViewBag.Roles = GetAvailableRoles();
                    return View(user);
                }

                // Hash password
                if (_hasher != null && !string.IsNullOrEmpty(user.Password))
                {
                    user.Password = _hasher.HashPassword(user, user.Password);
                }

                _context.Users.Add(user);

                // If created as Intern, auto-create Intern record if missing
                if (user.Role == "Intern")
                {
                    bool internExists = await _context.Interns.AnyAsync(i => i.Email == user.Email);
                    if (!internExists)
                    {
                        _context.Interns.Add(new Intern
                        {
                            Name = user.FullName,
                            Email = user.Email
                        });
                    }
                }

                // If created as Mentor, auto-create Mentor record if missing
                if (user.Role == "Mentor")
                {
                    bool mentorExists = await _context.Mentors.AnyAsync(m => m.Email == user.Email);
                    if (!mentorExists)
                    {
                        _context.Mentors.Add(new Mentor
                        {
                            FullName = user.FullName,
                            Email = user.Email,
                            Status = "Active"
                        });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = GetAvailableRoles();
            return View(user);
        }

        // GET: Manage Roles
        [HttpGet]
        public async Task<IActionResult> ManageRoles()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // POST: Change Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Prevent self-demotion of currently logged in admin
            if (user.Id.ToString() == currentUserId && role != "Admin")
            {
                TempData["Error"] = "You cannot remove the Admin role from your own active account.";
                return RedirectToAction(nameof(ManageRoles));
            }

            user.Role = role;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Role for {user.FullName} updated to {role}.";
            return RedirectToAction(nameof(ManageRoles));
        }

        // GET: Edit User
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = GetAvailableRoles();
            return View(user);
        }

        // POST: Edit User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user, string? NewPassword)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);

            if (existingUser == null)
            {
                return NotFound();
            }

            bool emailExists = await _context.Users.AnyAsync(x => x.Email == user.Email && x.Id != user.Id);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                ViewBag.Roles = GetAvailableRoles();
                return View(user);
            }

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;

            // Only update/rehash password if a new raw password was supplied
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                existingUser.Password = _hasher != null
                    ? _hasher.HashPassword(existingUser, NewPassword)
                    : NewPassword;
            }

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete User Confirmation
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Delete User
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Prevent self-deletion
            if (user.Id.ToString() == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own active account.";
                return RedirectToAction(nameof(Index));
            }

            // Also clean up associated Intern profile if applicable
            if (user.Role == "Intern")
            {
                var intern = await _context.Interns.FirstOrDefaultAsync(i => i.Email == user.Email);
                if (intern != null)
                {
                    _context.Interns.Remove(intern);
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}