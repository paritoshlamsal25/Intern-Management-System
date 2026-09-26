using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class Mentor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; } // e.g., Senior Software Engineer

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive

        // Navigation Property: One Mentor guides many Interns
        public ICollection<Intern> Interns { get; set; } = new List<Intern>();
    }
}