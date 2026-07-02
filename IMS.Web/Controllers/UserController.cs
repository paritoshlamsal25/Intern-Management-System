using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace IMS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(x =>
                    x.FullName.Contains(search) ||
                    x.Email.Contains(search) ||
                    x.Role.Contains(search));
            }
            ViewBag.Search = search;

            return View(users.ToList());
        }
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string>
    {
        "Admin",
        "HR"
    };

            return View();
        }
        [HttpPost]
        public IActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                bool emailExists = _context.Users.Any(x => x.Email == user.Email);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email already exists.");

                    ViewBag.Roles = new List<string>
            {
                "Admin",
                "HR"
            };

                    return View(user);
                }

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "User created successfully.";

                return RedirectToAction("Index");
            }

            ViewBag.Roles = new List<string>
    {
        "Admin",
        "HR"
    };

            return View(user);
        }
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = new List<string>
    {
        "Admin",
        "HR"
    };

            return View(user);
        }
        [HttpPost]
        public IActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                bool emailExists = _context.Users.Any(x => x.Email == user.Email && x.Id != user.Id);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email already exists.");

                    ViewBag.Roles = new List<string>
            {
                "Admin",
                "HR"
            };

                    return View(user);
                }

                _context.Users.Update(user);
                _context.SaveChanges();

                TempData["Success"] = "User updated successfully.";

                return RedirectToAction("Index");
            }

            ViewBag.Roles = new List<string>
    {
        "Admin",
        "HR"
    };

            return View(user);
        }
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["Success"] = "User deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}