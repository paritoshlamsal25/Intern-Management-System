using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Web.Models;

namespace IMS.Web.Services
{
    /// <summary>
    /// Business logic contract for managing intern progress reports.
    /// </summary>
    public interface IProgressReportService
    {
        Task<IEnumerable<ProgressReport>> GetByFilterAsync(int? internId, DateTime? from, DateTime? to);
        Task<ProgressReport?> GetByIdAsync(int id);
        Task<ProgressReport> CreateAsync(ProgressReport report, string performedBy = "System");
        Task<bool> UpdateAsync(ProgressReport report, string performedBy = "System");
        Task<bool> DeleteAsync(int id, string performedBy = "System");
    }
}