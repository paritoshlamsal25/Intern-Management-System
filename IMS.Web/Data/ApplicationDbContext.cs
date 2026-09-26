using Microsoft.EntityFrameworkCore;
using IMS.Web.Models;

namespace IMS.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Intern> Interns { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Mentor> Mentors { get; set; } = null!;
        public DbSet<TaskItem> TaskItems { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
        public DbSet<ProgressReport> ProgressReports { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- INDEXES & CONSTRAINTS ---

            // Unique index for User Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Unique index for Intern Email
            modelBuilder.Entity<Intern>()
                .HasIndex(i => i.Email)
                .IsUnique();

            // Performance index on AuditLog timestamp for fast log querying
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.Timestamp);

            // --- ENTITY RELATIONSHIPS & CASCADE DELETE BEHAVIOR ---

            // 1. Intern -> Department (Set Null on Department Delete)
            modelBuilder.Entity<Intern>()
                .HasOne(i => i.Department)
                .WithMany(d => d.Interns)
                .HasForeignKey(i => i.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // 2. Intern -> Mentor (Set Null on Mentor Delete)
            modelBuilder.Entity<Intern>()
                .HasOne(i => i.Mentor)
                .WithMany(m => m.Interns)
                .HasForeignKey(i => i.MentorId)
                .OnDelete(DeleteBehavior.SetNull);

            // 3. TaskItem -> Intern (Restrict to avoid cascading delete issues)
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Intern)
                .WithMany(i => i.Tasks)
                .HasForeignKey(t => t.InternId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Attendance -> Intern (Restrict delete)
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Intern)
                .WithMany(i => i.Attendances)
                .HasForeignKey(a => a.InternId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. LeaveRequest -> Intern (Restrict delete)
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Intern)
                .WithMany(i => i.LeaveRequests)
                .HasForeignKey(l => l.InternId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. ProgressReport -> Intern (Restrict delete)
            modelBuilder.Entity<ProgressReport>()
                .HasOne(p => p.Intern)
                .WithMany(i => i.ProgressReports)
                .HasForeignKey(p => p.InternId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. ProgressReport -> TaskItem (SetNull: deleting a task doesn't delete the report)
            modelBuilder.Entity<ProgressReport>()
                .HasOne(p => p.Task)
                .WithMany()
                .HasForeignKey(p => p.TaskId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}