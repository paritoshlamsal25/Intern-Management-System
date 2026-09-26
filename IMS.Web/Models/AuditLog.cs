using System;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Action Executed")]
        public string Action { get; set; } = string.Empty; // e.g., "Create", "Update", "Delete", "LoginFailed"

        [Required]
        [StringLength(100)]
        [Display(Name = "Entity / Module")]
        public string Entity { get; set; } = string.Empty; // e.g., "Intern", "TaskItem", "User", "LeaveRequest"

        [Display(Name = "Entity ID")]
        public int? EntityId { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Performed By")]
        public string PerformedBy { get; set; } = string.Empty; // User's email or username

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(2000)]
        [Display(Name = "Action Details / Changes")]
        public string? Details { get; set; } // Detailed message or JSON string of changes

        [StringLength(45)]
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }
    }
}