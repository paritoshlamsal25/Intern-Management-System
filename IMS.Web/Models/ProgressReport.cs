using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Web.Models
{
    public class ProgressReport
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select an intern.")]
        [Display(Name = "Intern")]
        public int InternId { get; set; }

        // Navigation Property
        public Intern? Intern { get; set; }

        // Optional: Link to a specific completed/submitted task
        [Display(Name = "Task / Assignment")]
        public int? TaskId { get; set; }

        public TaskItem? Task { get; set; }

        [Required(ErrorMessage = "Report date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Today;

        // Topic being evaluated (e.g., "React Basics", "API Design", etc.)
        [StringLength(200, ErrorMessage = "Topic cannot exceed 200 characters.")]
        [Display(Name = "Topic / Evaluation Area")]
        public string? Topic { get; set; }

        // Marks awarded
        [Range(0, 10000, ErrorMessage = "Marks must be between 0 and the maximum.")]
        [Display(Name = "Marks Obtained")]
        public int? Marks { get; set; }

        // Maximum marks for this evaluation
        [Range(1, 10000, ErrorMessage = "Maximum marks must be at least 1.")]
        [Display(Name = "Out Of (Max Marks)")]
        public int? MaxMarks { get; set; }

        // Free-text resource description if no task is linked (e.g., overdue scenario)
        [StringLength(500)]
        [Display(Name = "Assignment / Resource Reference")]
        public string? AssignmentReference { get; set; }

        [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
        [Display(Name = "Mentor Notes / Feedback")]
        public string? Notes { get; set; }

        // Status is kept in DB for backward compat but hidden from UI
        [StringLength(50)]
        public string Status { get; set; } = "Completed";
    }
}