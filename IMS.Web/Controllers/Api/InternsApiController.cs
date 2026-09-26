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
    public class InternsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InternsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTOs to decouple API responses from database entity models
        public record InternResponseDto(
            int Id,
            string Name,
            string Email,
            string? Phone,
            int? DepartmentId,
            string? DepartmentName,
            int? MentorId,
            string? MentorName,
            string Status
        );

        public record CreateInternDto(
            string Name,
            string Email,
            string? Phone,
            int? DepartmentId,
            int? MentorId,
            string Status = "Active"
        );

        // GET: api/InternsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InternResponseDto>>> GetAll()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
            var isIntern = User.IsInRole("Intern");

            IQueryable<Intern> query = _context.Interns
                .Include(i => i.Department)
                .Include(i => i.Mentor);

            // Restrict intern user role to viewing only their own record
            if (isIntern && !string.IsNullOrEmpty(userEmail))
            {
                query = query.Where(i => i.Email == userEmail);
            }

            var interns = await query
                .Select(i => new InternResponseDto(
                    i.Id,
                    i.Name,
                    i.Email,
                    i.Phone,
                    i.DepartmentId,
                    i.Department != null ? i.Department.Name : null,
                    i.MentorId,
                    i.Mentor != null ? i.Mentor.FullName : null,
                    i.Status
                ))
                .ToListAsync();

            return Ok(interns);
        }

        // GET: api/InternsApi/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<InternResponseDto>> Get(int id)
        {
            var intern = await _context.Interns
                .Include(i => i.Department)
                .Include(i => i.Mentor)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (intern == null)
            {
                return NotFound(new { message = $"Intern with ID {id} was not found." });
            }

            // Ownership check for Intern role
            if (User.IsInRole("Intern"))
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
                if (intern.Email != userEmail)
                {
                    return Forbid();
                }
            }

            var dto = new InternResponseDto(
                intern.Id,
                intern.Name,
                intern.Email,
                intern.Phone,
                intern.DepartmentId,
                intern.Department?.Name,
                intern.MentorId,
                intern.Mentor?.FullName,
                intern.Status
            );

            return Ok(dto);
        }

        // POST: api/InternsApi
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<InternResponseDto>> Create([FromBody] CreateInternDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check for existing duplicate email
            var emailExists = await _context.Interns.AnyAsync(i => i.Email == dto.Email);
            if (emailExists)
            {
                return BadRequest(new { message = $"An intern with email '{dto.Email}' already exists." });
            }

            var intern = new Intern
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                DepartmentId = dto.DepartmentId,
                MentorId = dto.MentorId,
                Status = dto.Status
            };

            _context.Interns.Add(intern);
            await _context.SaveChangesAsync();

            // Load navigation references for complete response DTO
            await _context.Entry(intern).Reference(i => i.Department).LoadAsync();
            await _context.Entry(intern).Reference(i => i.Mentor).LoadAsync();

            var responseDto = new InternResponseDto(
                intern.Id,
                intern.Name,
                intern.Email,
                intern.Phone,
                intern.DepartmentId,
                intern.Department?.Name,
                intern.MentorId,
                intern.Mentor?.FullName,
                intern.Status
            );

            return CreatedAtAction(nameof(Get), new { id = intern.Id }, responseDto);
        }
    }
}