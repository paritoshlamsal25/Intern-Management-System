using System;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select an intern.")]
        [Display(Name = "Intern")]
        public int InternId { get; set; }

        // Navigation property
        public Intern? Intern { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Please specify a reason for leave.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [DataType(DataType.DateTime)]
        [Display(Name = "Applied On")]
        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        [Display(Name = "Approved / Processed By")]
        public string? ApprovedBy { get; set; }
    }
}