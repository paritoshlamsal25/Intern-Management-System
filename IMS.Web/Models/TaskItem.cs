using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public int InternId { get; set; }

        public Intern? Intern { get; set; }
    }
}