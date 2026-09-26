using System;
using System.Threading;
using System.Threading.Tasks;
using IMS.Web.Data;
using IMS.Web.Models;

namespace IMS.Web.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            string action,
            string entity = "",
            int? entityId = null,
            string? details = null,
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            var auditLog = new AuditLog
            {
                Action = action ?? string.Empty,
                Entity = entity ?? string.Empty,
                EntityId = entityId,
                PerformedBy = userId ?? "System",
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}