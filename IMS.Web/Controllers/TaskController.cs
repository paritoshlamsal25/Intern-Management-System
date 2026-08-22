using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var tasks = _context.TaskItems
                .Include(x => x.Intern)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                tasks = tasks.Where(x =>
                    x.Title.Contains(search) ||
                    x.Intern!.Name.Contains(search));
            }

            ViewBag.Search = search;

            return View(tasks.ToList());
        }
        public IActionResult Create()
        {
            ViewBag.Interns = _context.Interns.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(TaskItem task)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Interns = _context.Interns.ToList();
                return View(task);
            }

            _context.TaskItems.Add(task);
            _context.SaveChanges();

            TempData["Success"] = "Task created successfully.";

            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var task = _context.TaskItems.Find(id);

            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Interns = _context.Interns.ToList();

            return View(task);
        }

        [HttpPost]
        public IActionResult Edit(TaskItem task)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Interns = _context.Interns.ToList();
                return View(task);
            }

            var existingTask = _context.TaskItems.Find(task.Id);

            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.DueDate = task.DueDate;
            existingTask.Status = task.Status;
            existingTask.InternId = task.InternId;

            _context.SaveChanges();

            TempData["Success"] = "Task updated successfully.";

            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var task = _context.TaskItems
                .Include(x => x.Intern)
                .FirstOrDefault(x => x.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var task = _context.TaskItems.Find(id);

            if (task == null)
            {
                return NotFound();
            }

            _context.TaskItems.Remove(task);
            _context.SaveChanges();

            TempData["Success"] = "Task deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}