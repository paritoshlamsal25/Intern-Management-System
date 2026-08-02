using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class Mentor
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? PhoneNumber { get; set; }

        public ICollection<Intern> Interns { get; set; } = new List<Intern>();
    }
}