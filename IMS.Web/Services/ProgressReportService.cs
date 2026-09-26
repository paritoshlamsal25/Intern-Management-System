using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Services
{
    public class ProgressReportService : IProgressReportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _auditService;

        public ProgressReportService(ApplicationDbContext db, IAuditService auditService)
        {
            _db = db;
            _auditService = auditService;
        }

        public async Task<IEnumerable<ProgressReport>> GetByFilterAsync(int? internId, DateTime? from, DateTime? to)
        {
            IQueryable<ProgressReport> qry = _db.ProgressReports
                .Include(p => p.Intern)
                .AsNoTracking();

            if (internId.HasValue && internId.Value > 0)
            {
                qry = qry.Where(p => p.InternId == internId.Value);
            }

            if (from.HasValue)
            {
                qry = qry.Where(p => p.Date >= from.Value.Date);
            }

            if (to.HasValue)
            {
                qry = qry.Where(p => p.Date <= to.Value.Date);
            }

            return await qry.OrderByDescending(p => p.Date).ToListAsync();
        }

        public async Task<ProgressReport?> GetByIdAsync(int id)
        {
            return await _db.ProgressReports
                .Include(p => p.Intern)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProgressReport> CreateAsync(ProgressReport report, string performedBy = "System")
        {
            if (report == null) throw new ArgumentNullException(nameof(report));

            _db.ProgressReports.Add(report);
            await _db.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "Create",
                entity: "ProgressReport",
                entityId: report.Id,
                userId: performedBy, // Changed from performedBy: to userId:
                details: $"Created progress report for Intern ID {report.InternId} with status '{report.Status}'."
            );

            return report;
        }

        public async Task<bool> UpdateAsync(ProgressReport report, string performedBy = "System")
        {
            if (report == null) throw new ArgumentNullException(nameof(report));

            var existing = await _db.ProgressReports.FindAsync(report.Id);
            if (existing == null) return false;

            existing.Status = report.Status;
            existing.Notes = report.Notes;
            existing.Date = report.Date;
            existing.Topic = report.Topic;
            existing.Marks = report.Marks;
            existing.MaxMarks = report.MaxMarks;
            existing.AssignmentReference = report.AssignmentReference;
            existing.TaskId = report.TaskId;

            _db.ProgressReports.Update(existing);
            await _db.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "Update",
                entity: "ProgressReport",
                entityId: report.Id,
                userId: performedBy, // Changed from performedBy: to userId:
                details: $"Updated progress report ID {report.Id}."
            );

            return true;
        }

        public async Task<bool> DeleteAsync(int id, string performedBy = "System")
        {
            var report = await _db.ProgressReports.FindAsync(id);
            if (report == null) return false;

            _db.ProgressReports.Remove(report);
            await _db.SaveChangesAsync();

            await _auditService.LogAsync(
                action: "Delete",
                entity: "ProgressReport",
                entityId: id,
                userId: performedBy, // Changed from performedBy: to userId:
                details: $"Deleted progress report ID {id}."
            );

            return true;
        }
    }
}