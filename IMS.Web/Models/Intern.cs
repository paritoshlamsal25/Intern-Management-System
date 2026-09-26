using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class Intern
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Intern name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string? Phone { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        [Display(Name = "Assigned Mentor")]
        public int? MentorId { get; set; }
        public Mentor? Mentor { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Terminated

        // Navigation Collections
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<ProgressReport> ProgressReports { get; set; } = new List<ProgressReport>();
    }
}