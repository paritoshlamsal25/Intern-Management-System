using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public int InternId { get; set; }

        public Intern? Intern { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";
    }
}