using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IMS.Web.Data;
using IMS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTOs to prevent circular JSON reference issues & over-posting vulnerabilities
        public record AttendanceResponseDto(
            int Id,
            int InternId,
            string InternName,
            string InternEmail,
            DateTime Date,
            string Status,
            string? Notes
        );

        public record CreateAttendanceDto(
            int InternId,
            DateTime Date,
            string Status,
            string? Notes
        );

        // GET: api/AttendanceApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceResponseDto>>> GetAll()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            var isIntern = User.IsInRole("Intern");

            IQueryable<Attendance> query = _context.Attendances.Include(a => a.Intern);

            // If caller is an Intern, restrict returned records to their own attendance
            if (isIntern && !string.IsNullOrEmpty(userEmail))
            {
                query = query.Where(a => a.Intern != null && a.Intern.Email == userEmail);
            }

            var list = await query
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceResponseDto(
                    a.Id,
                    a.InternId,
                    a.Intern != null ? a.Intern.Name : "N/A",
                    a.Intern != null ? a.Intern.Email : "N/A",
                    a.Date,
                    a.Status,
                    a.Notes
                ))
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/AttendanceApi/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AttendanceResponseDto>> Get(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Intern)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound(new { message = $"Attendance record with ID {id} was not found." });
            }

            // Ownership check for Intern role
            if (User.IsInRole("Intern"))
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
                if (attendance.Intern == null || attendance.Intern.Email != userEmail)
                {
                    return Forbid();
                }
            }

            var dto = new AttendanceResponseDto(
                attendance.Id,
                attendance.InternId,
                attendance.Intern?.Name ?? "N/A",
                attendance.Intern?.Email ?? "N/A",
                attendance.Date,
                attendance.Status,
                attendance.Notes
            );

            return Ok(dto);
        }

        // POST: api/AttendanceApi
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<AttendanceResponseDto>> Create([FromBody] CreateAttendanceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify the intern actually exists
            var internExists = await _context.Interns.AnyAsync(i => i.Id == dto.InternId);
            if (!internExists)
            {
                return BadRequest(new { message = $"Intern with ID {dto.InternId} does not exist." });
            }

            var attendance = new Attendance
            {
                InternId = dto.InternId,
                Date = dto.Date,
                Status = dto.Status,
                Notes = dto.Notes
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            // Fetch intern details to populate response DTO
            var intern = await _context.Interns.FindAsync(dto.InternId);

            var responseDto = new AttendanceResponseDto(
                attendance.Id,
                attendance.InternId,
                intern?.Name ?? "N/A",
                intern?.Email ?? "N/A",
                attendance.Date,
                attendance.Status,
                attendance.Notes
            );

            return CreatedAtAction(nameof(Get), new { id = attendance.Id }, responseDto);
        }
    }
}