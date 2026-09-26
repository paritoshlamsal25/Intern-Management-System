using System;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select an intern.")]
        [Display(Name = "Intern")]
        public int InternId { get; set; }

        // Navigation property
        public Intern? Intern { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Attendance status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = "Present"; // E.g., Present, Absent, Late, On Leave

        [StringLength(250, ErrorMessage = "Notes cannot exceed 250 characters.")]
        public string? Notes { get; set; }
    }
}