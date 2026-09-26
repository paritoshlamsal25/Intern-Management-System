using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Department code cannot exceed 10 characters.")]
        [Display(Name = "Department Code")]
        public string? Code { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        // Navigation Property: One Department has many Interns
        public ICollection<Intern> Interns { get; set; } = new List<Intern>();
    }
}