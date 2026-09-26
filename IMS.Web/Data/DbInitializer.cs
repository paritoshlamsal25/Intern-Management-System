using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IMS.Web.Models;
using IMS.Web.Services;

namespace IMS.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();

            // Ensure all required tables and columns exist in SQL Server LocalDB
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception)
            {
                // Fallback: If migrations metadata has conflict, ensure schema consistency via SQL
            }

            try
            {
                var sql = @"
-- USERS
IF OBJECT_ID(N'[Users]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'CreatedAt')
        ALTER TABLE [Users] ADD [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT GETUTCDATE();
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'LastLogin')
        ALTER TABLE [Users] ADD [LastLogin] datetime2 NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'FullName')
        ALTER TABLE [Users] ADD [FullName] nvarchar(100) NOT NULL CONSTRAINT [DF_Users_FullName] DEFAULT N'';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'Role')
        ALTER TABLE [Users] ADD [Role] nvarchar(20) NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT N'Intern';
END

-- INTERNS
IF OBJECT_ID(N'[Interns]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Interns]') AND name = N'Phone')
        ALTER TABLE [Interns] ADD [Phone] nvarchar(20) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Interns]') AND name = N'StartDate')
        ALTER TABLE [Interns] ADD [StartDate] datetime2 NOT NULL CONSTRAINT [DF_Interns_StartDate] DEFAULT GETUTCDATE();
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Interns]') AND name = N'EndDate')
        ALTER TABLE [Interns] ADD [EndDate] datetime2 NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Interns]') AND name = N'Status')
        ALTER TABLE [Interns] ADD [Status] nvarchar(20) NOT NULL CONSTRAINT [DF_Interns_Status] DEFAULT N'Active';
END

-- DEPARTMENTS
IF OBJECT_ID(N'[Departments]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Departments]') AND name = N'Code')
        ALTER TABLE [Departments] ADD [Code] nvarchar(10) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Departments]') AND name = N'Description')
        ALTER TABLE [Departments] ADD [Description] nvarchar(250) NULL;
END

-- MENTORS
IF OBJECT_ID(N'[Mentors]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Mentors]') AND name = N'PhoneNumber')
        ALTER TABLE [Mentors] ADD [PhoneNumber] nvarchar(20) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Mentors]') AND name = N'Designation')
        ALTER TABLE [Mentors] ADD [Designation] nvarchar(100) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Mentors]') AND name = N'Department')
        ALTER TABLE [Mentors] ADD [Department] nvarchar(100) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Mentors]') AND name = N'Status')
        ALTER TABLE [Mentors] ADD [Status] nvarchar(20) NOT NULL CONSTRAINT [DF_Mentors_Status] DEFAULT N'Active';
END

-- TASKITEMS
IF OBJECT_ID(N'[TaskItems]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TaskItems]') AND name = N'CreatedDate')
        ALTER TABLE [TaskItems] ADD [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_TaskItems_CreatedDate] DEFAULT GETUTCDATE();
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TaskItems]') AND name = N'CompletedDate')
        ALTER TABLE [TaskItems] ADD [CompletedDate] datetime2 NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TaskItems]') AND name = N'AttachmentPath')
        ALTER TABLE [TaskItems] ADD [AttachmentPath] nvarchar(500) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TaskItems]') AND name = N'AttachmentFileName')
        ALTER TABLE [TaskItems] ADD [AttachmentFileName] nvarchar(255) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TaskItems]') AND name = N'SubmissionNote')
        ALTER TABLE [TaskItems] ADD [SubmissionNote] nvarchar(2000) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TaskItems]') AND name = N'SubmissionFilePath')
        ALTER TABLE [TaskItems] ADD [SubmissionFilePath] nvarchar(500) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TaskItems]') AND name = N'SubmissionFileName')
        ALTER TABLE [TaskItems] ADD [SubmissionFileName] nvarchar(255) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TaskItems]') AND name = N'SubmittedDate')
        ALTER TABLE [TaskItems] ADD [SubmittedDate] datetime2 NULL;
END

-- LEAVEREQUESTS
IF OBJECT_ID(N'[LeaveRequests]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[LeaveRequests]') AND name = N'AppliedOn')
        ALTER TABLE [LeaveRequests] ADD [AppliedOn] datetime2 NOT NULL CONSTRAINT [DF_LeaveRequests_AppliedOn] DEFAULT GETUTCDATE();
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[LeaveRequests]') AND name = N'ApprovedBy')
        ALTER TABLE [LeaveRequests] ADD [ApprovedBy] nvarchar(100) NULL;
END

-- ATTENDANCES
IF OBJECT_ID(N'[Attendances]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Attendances]') AND name = N'Notes')
        ALTER TABLE [Attendances] ADD [Notes] nvarchar(250) NULL;
END

-- PROGRESSREPORTS
IF OBJECT_ID(N'[ProgressReports]', N'U') IS NULL
BEGIN
    CREATE TABLE [ProgressReports] (
        [Id] int NOT NULL IDENTITY,
        [InternId] int NOT NULL,
        [Date] datetime2 NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [HoursWorked] float NULL,
        CONSTRAINT [PK_ProgressReports] PRIMARY KEY ([Id])
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressReports]') AND name = N'HoursWorked')
        ALTER TABLE [ProgressReports] ADD [HoursWorked] float NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressReports]') AND name = N'Notes')
        ALTER TABLE [ProgressReports] ADD [Notes] nvarchar(1000) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressReports]') AND name = N'Status')
        ALTER TABLE [ProgressReports] ADD [Status] nvarchar(50) NOT NULL CONSTRAINT [DF_ProgressReports_Status] DEFAULT N'In Progress';
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ProgressReports]') AND name = N'Date')
        ALTER TABLE [ProgressReports] ADD [Date] datetime2 NOT NULL CONSTRAINT [DF_ProgressReports_Date] DEFAULT GETUTCDATE();
END

-- AUDITLOGS
IF OBJECT_ID(N'[AuditLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [Action] nvarchar(100) NOT NULL,
        [Entity] nvarchar(100) NOT NULL,
        [EntityId] int NULL,
        [PerformedBy] nvarchar(256) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [Details] nvarchar(2000) NULL,
        [IpAddress] nvarchar(45) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END
";
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception)
            {
                // Continue with seeding
            }

            // 1. Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                await context.Departments.AddRangeAsync(
                    new Department { Name = "Software Engineering", Code = "ENG", Description = "Software Development & QA" },
                    new Department { Name = "Human Resources", Code = "HR", Description = "Talent Acquisition & Management" },
                    new Department { Name = "Product Management", Code = "PM", Description = "Product Planning & Operations" }
                );
                await context.SaveChangesAsync();
            }

            // 2. Seed Mentors
            if (!await context.Mentors.AnyAsync())
            {
                await context.Mentors.AddAsync(new Mentor
                {
                    FullName = "John Doe",
                    Email = "mentor@local",
                    PhoneNumber = "+1234567890",
                    Designation = "Senior Software Engineer",
                    Department = "Software Engineering",
                    Status = "Active"
                });
                await context.SaveChangesAsync();
            }

            // 3. Seed Default System Users
            if (!await context.Users.AnyAsync(u => u.Email == "admin@local"))
            {
                var admin = new User
                {
                    FullName = "Administrator",
                    Email = "admin@local",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                admin.Password = hasher.HashPassword(admin, "admin123");
                await context.Users.AddAsync(admin);
            }

            if (!await context.Users.AnyAsync(u => u.Email == "hr@local"))
            {
                var hr = new User
                {
                    FullName = "HR Manager",
                    Email = "hr@local",
                    Role = "HR",
                    CreatedAt = DateTime.UtcNow
                };
                hr.Password = hasher.HashPassword(hr, "hr123");
                await context.Users.AddAsync(hr);
            }

            if (!await context.Users.AnyAsync(u => u.Email == "mentor@local"))
            {
                var mentorUser = new User
                {
                    FullName = "John Doe",
                    Email = "mentor@local",
                    Role = "Mentor",
                    CreatedAt = DateTime.UtcNow
                };
                mentorUser.Password = hasher.HashPassword(mentorUser, "mentor123");
                await context.Users.AddAsync(mentorUser);
            }

            if (!await context.Users.AnyAsync(u => u.Email == "intern@local"))
            {
                var internUser = new User
                {
                    FullName = "Sample Intern",
                    Email = "intern@local",
                    Role = "Intern",
                    CreatedAt = DateTime.UtcNow
                };
                internUser.Password = hasher.HashPassword(internUser, "intern123");
                await context.Users.AddAsync(internUser);
            }

            await context.SaveChangesAsync();

            // 4. Seed Corresponding Intern Entity
            if (!await context.Interns.AnyAsync(i => i.Email == "intern@local"))
            {
                var defaultDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "ENG");
                var defaultMentor = await context.Mentors.FirstOrDefaultAsync(m => m.Email == "mentor@local" || m.Email == "johndoe@local");

                var sampleIntern = new Intern
                {
                    Name = "Sample Intern",
                    Email = "intern@local",
                    Phone = "+1987654321",
                    DepartmentId = defaultDept?.Id,
                    MentorId = defaultMentor?.Id,
                    StartDate = DateTime.Today,
                    Status = "Active"
                };

                await context.Interns.AddAsync(sampleIntern);
                await context.SaveChangesAsync();
            }

            // 5. Seed Initial Progress Report
            if (!await context.ProgressReports.AnyAsync())
            {
                var internForReport = await context.Interns.FirstOrDefaultAsync(i => i.Email == "intern@local");

                if (internForReport != null)
                {
                    await context.ProgressReports.AddAsync(new ProgressReport
                    {
                        InternId = internForReport.Id,
                        Date = DateTime.Today,
                        Status = "Completed",
                        Notes = "Initial setup and project onboarding completed successfully.",
                        Topic = "Onboarding",
                        Marks = 85,
                        MaxMarks = 100
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}