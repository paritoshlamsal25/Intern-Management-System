using System.ComponentModel.DataAnnotations;
namespace IMS.Web.Models
{
    public class Intern
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}