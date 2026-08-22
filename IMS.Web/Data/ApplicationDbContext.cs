using Microsoft.EntityFrameworkCore;
using IMS.Web.Models;

namespace IMS.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Intern> Interns { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Mentor> Mentors { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
    }
}