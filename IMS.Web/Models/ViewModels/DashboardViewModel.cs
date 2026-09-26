namespace IMS.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public AdminDashboardViewModel? AdminData { get; set; }
        public HRDashboardViewModel? HRData { get; set; }
        public MentorDashboardViewModel? MentorData { get; set; }
        public InternDashboardViewModel? InternData { get; set; }
    }

    public class MentorDashboardViewModel
    {
        public int MentorId { get; set; }
        public string MentorName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int AssignedInternsCount { get; set; }
        public int ActiveTasksCount { get; set; }
        public int SubmittedTasksCount { get; set; }
        public int CompletedTasksCount { get; set; }
        public List<IMS.Web.Models.Intern> MyInterns { get; set; } = new();
        public List<IMS.Web.Models.TaskItem> RecentTaskSubmissions { get; set; } = new();
        public List<IMS.Web.Models.TaskItem> ActiveTasks { get; set; } = new();
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalInterns { get; set; }
        public int TotalTasks { get; set; }
        public int TotalAttendance { get; set; }
        public int TotalLeaves { get; set; }
        public int PendingLeavesCount { get; set; }
        public int PendingTasksCount { get; set; }
        public int CompletedTasksCount { get; set; }
        public List<IMS.Web.Models.LeaveRequest> PendingLeaves { get; set; } = new();
        public List<IMS.Web.Models.Intern> RecentInterns { get; set; } = new();
    }

    public class HRDashboardViewModel
    {
        public int ActiveInternsCount { get; set; }
        public int PendingLeavesCount { get; set; }
        public int PendingTasksCount { get; set; }
        public int TodayPresentCount { get; set; }
        public List<IMS.Web.Models.LeaveRequest> PendingLeaves { get; set; } = new();
        public List<IMS.Web.Models.Intern> RecentInterns { get; set; } = new();
    }

    public class InternDashboardViewModel
    {
        public int InternId { get; set; }
        public string InternName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string MentorName { get; set; } = string.Empty;
        public bool HasClockedInToday { get; set; }
        public int AssignedTasksCount { get; set; }
        public int PendingTasksCount { get; set; }
        public int CompletedTasksCount { get; set; }
        public List<IMS.Web.Models.TaskItem> MyPendingTasks { get; set; } = new();
        public List<IMS.Web.Models.LeaveRequest> MyLeaveRequests { get; set; } = new();
        public List<IMS.Web.Models.ProgressReport> MyProgressReports { get; set; } = new();
    }
}