using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Optional HTTP status code associated with the error (e.g., 404, 500, 403).
        /// </summary>
        public int StatusCode { get; set; } = 500;

        /// <summary>
        /// User-friendly error message to display on the UI.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Optional detailed description for debugging or logging display.
        /// </summary>
        public string? Description { get; set; }
    }
}