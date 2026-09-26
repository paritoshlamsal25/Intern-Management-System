using System;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Task title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        [Display(Name = "Task Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [Display(Name = "Description / Instructions")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Task status is required.")]
        [StringLength(30, ErrorMessage = "Status cannot exceed 30 characters.")]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Overdue

        [Required(ErrorMessage = "Please assign this task to an intern.")]
        [Display(Name = "Assigned Intern")]
        public int InternId { get; set; }

        // Navigation Property
        public Intern? Intern { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        // Mentor Assignment Attachment
        [Display(Name = "Assignment Attachment")]
        public string? AttachmentPath { get; set; }

        [Display(Name = "Attachment File Name")]
        public string? AttachmentFileName { get; set; }

        // Intern Task Submission & Progress
        [Display(Name = "Submission Notes / Progress Message")]
        [StringLength(2000, ErrorMessage = "Submission note cannot exceed 2000 characters.")]
        public string? SubmissionNote { get; set; }

        [Display(Name = "Submission File")]
        public string? SubmissionFilePath { get; set; }

        [Display(Name = "Submission File Name")]
        public string? SubmissionFileName { get; set; }

        [Display(Name = "Submitted Date")]
        public DateTime? SubmittedDate { get; set; }
    }
}