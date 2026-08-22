using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int InternId { get; set; }

        public Intern? Intern { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } = "Present";
    }
}