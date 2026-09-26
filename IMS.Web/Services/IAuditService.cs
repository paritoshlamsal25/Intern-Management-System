using System.Threading;
using System.Threading.Tasks;

namespace IMS.Web.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            string action,
            string entity = "",
            int? entityId = null,
            string? details = null,
            string? userId = null,
            CancellationToken cancellationToken = default);
    }
}