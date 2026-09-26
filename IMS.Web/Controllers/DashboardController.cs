using IMS.Web.Data;
using IMS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            if (User.IsInRole("Admin"))
            {
                viewModel.AdminData = new AdminDashboardViewModel
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalDepartments = await _context.Departments.CountAsync(),
                    TotalInterns = await _context.Interns.CountAsync(),
                    TotalTasks = await _context.TaskItems.CountAsync(),
                    TotalAttendance = await _context.Attendances.CountAsync(),
                    TotalLeaves = await _context.LeaveRequests.CountAsync(),
                    PendingLeavesCount = await _context.LeaveRequests.CountAsync(l => l.Status == "Pending"),
                    PendingTasksCount = await _context.TaskItems.CountAsync(t => t.Status == "Pending" || t.Status == "In Progress"),
                    CompletedTasksCount = await _context.TaskItems.CountAsync(t => t.Status == "Completed"),
                    PendingLeaves = await _context.LeaveRequests
                        .Include(l => l.Intern)
                        .Where(l => l.Status == "Pending")
                        .OrderByDescending(l => l.Id)
                        .Take(5)
                        .ToListAsync(),
                    RecentInterns = await _context.Interns
                        .Include(i => i.Department)
                        .Include(i => i.Mentor)
                        .OrderByDescending(i => i.Id)
                        .Take(5)
                        .ToListAsync()
                };
            }
            else if (User.IsInRole("HR"))
            {
                viewModel.HRData = new HRDashboardViewModel
                {
                    ActiveInternsCount = await _context.Interns.CountAsync(i => i.Status == "Active"),
                    PendingLeavesCount = await _context.LeaveRequests.CountAsync(l => l.Status == "Pending"),
                    PendingTasksCount = await _context.TaskItems.CountAsync(t => t.Status == "Pending" || t.Status == "In Progress"),
                    TodayPresentCount = await _context.Attendances.CountAsync(a => a.Date.Date == DateTime.Today && a.Status == "Present"),
                    PendingLeaves = await _context.LeaveRequests
                        .Include(l => l.Intern)
                        .Where(l => l.Status == "Pending")
                        .OrderByDescending(l => l.Id)
                        .Take(5)
                        .ToListAsync(),
                    RecentInterns = await _context.Interns
                        .Include(i => i.Department)
                        .Include(i => i.Mentor)
                        .OrderByDescending(i => i.Id)
                        .Take(5)
                        .ToListAsync()
                };
            }
            else if (User.IsInRole("Mentor"))
            {
                var mentorEmail = User.FindFirstValue(ClaimTypes.Email);
                var mentor = await _context.Mentors
                    .Include(m => m.Interns)
                        .ThenInclude(i => i.Department)
                    .FirstOrDefaultAsync(m => m.Email == mentorEmail);

                var mentorId = mentor?.Id ?? 0;
                var myInternIds = mentor != null
                    ? mentor.Interns.Select(i => i.Id).ToList()
                    : new List<int>();

                viewModel.MentorData = new MentorDashboardViewModel
                {
                    MentorId = mentorId,
                    MentorName = mentor?.FullName ?? User.Identity?.Name ?? "Mentor",
                    Designation = mentor?.Designation ?? "Mentor / Senior Engineer",
                    Department = mentor?.Department ?? "Software Engineering",
                    AssignedInternsCount = myInternIds.Count,
                    ActiveTasksCount = await _context.TaskItems.CountAsync(t => myInternIds.Contains(t.InternId) && (t.Status == "Pending" || t.Status == "In Progress")),
                    SubmittedTasksCount = await _context.TaskItems.CountAsync(t => myInternIds.Contains(t.InternId) && (t.SubmittedDate != null || !string.IsNullOrEmpty(t.SubmissionFilePath))),
                    CompletedTasksCount = await _context.TaskItems.CountAsync(t => myInternIds.Contains(t.InternId) && t.Status == "Completed"),
                    MyInterns = mentor?.Interns.ToList() ?? new(),
                    RecentTaskSubmissions = await _context.TaskItems
                        .Include(t => t.Intern)
                        .Where(t => myInternIds.Contains(t.InternId) && (t.SubmittedDate != null || !string.IsNullOrEmpty(t.SubmissionFilePath) || !string.IsNullOrEmpty(t.SubmissionNote)))
                        .OrderByDescending(t => t.SubmittedDate ?? t.CreatedDate)
                        .Take(5)
                        .ToListAsync(),
                    ActiveTasks = await _context.TaskItems
                        .Include(t => t.Intern)
                        .Where(t => myInternIds.Contains(t.InternId) && t.Status != "Completed")
                        .OrderBy(t => t.DueDate)
                        .Take(5)
                        .ToListAsync()
                };
            }
            else if (User.IsInRole("Intern"))
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var intern = await _context.Interns
                    .Include(i => i.Department)
                    .Include(i => i.Mentor)
                    .FirstOrDefaultAsync(i => i.Email == userEmail);

                var internId = intern?.Id ?? 0;

                viewModel.InternData = new InternDashboardViewModel
                {
                    InternId = internId,
                    InternName = intern?.Name ?? User.Identity?.Name ?? "Intern",
                    DepartmentName = intern?.Department?.Name ?? "Unassigned",
                    MentorName = intern?.Mentor?.FullName ?? "Unassigned",
                    HasClockedInToday = internId > 0 && await _context.Attendances.AnyAsync(a => a.InternId == internId && a.Date.Date == DateTime.Today),
                    AssignedTasksCount = internId > 0 ? await _context.TaskItems.CountAsync(t => t.InternId == internId) : 0,
                    PendingTasksCount = internId > 0 ? await _context.TaskItems.CountAsync(t => t.InternId == internId && (t.Status == "Pending" || t.Status == "In Progress")) : 0,
                    CompletedTasksCount = internId > 0 ? await _context.TaskItems.CountAsync(t => t.InternId == internId && t.Status == "Completed") : 0,
                    MyPendingTasks = internId > 0 ? await _context.TaskItems
                        .Where(t => t.InternId == internId)
                        .OrderBy(t => t.DueDate)
                        .Take(5)
                        .ToListAsync() : new(),
                    MyLeaveRequests = internId > 0 ? await _context.LeaveRequests
                        .Where(l => l.InternId == internId)
                        .OrderByDescending(l => l.Id)
                        .Take(5)
                        .ToListAsync() : new(),
                    MyProgressReports = internId > 0 ? await _context.ProgressReports
                        .Where(r => r.InternId == internId)
                        .OrderByDescending(r => r.Date)
                        .Take(5)
                        .ToListAsync() : new()
                };
            }

            return View(viewModel);
        }
    }
}