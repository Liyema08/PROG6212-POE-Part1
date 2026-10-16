using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: /api/events/{eventId}/enrol
        [HttpPost("events/{eventId}/enrol")]
        public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolDto dto)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsParticipant(HttpContext))
                return StatusCode(403, new { message = "Only Participants can enrol" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return BadRequest(new { message = "Participant profile not found" });

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null || category.EventId != eventId)
                return NotFound(new { message = "Category not found for this event" });

            var existing = await _context.EventEnrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == participant.ParticipantId
                                       && e.EventId == eventId);

            if (existing != null)
                return Conflict(new { message = "You are already enrolled in this event" });

            var enrolment = new EventEnrolment
            {
                ParticipantId = participant.ParticipantId,
                EventId = eventId,
                CategoryId = dto.CategoryId,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                Notes = dto.Notes,
                EnrolmentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrolments), new { id = enrolment.EnrolmentId }, enrolment);
        }

        // GET: /api/enrolments/me
        [HttpGet("enrolments/me")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsParticipant(HttpContext))
                return StatusCode(403, new { message = "Only Participants can view their enrolments" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return BadRequest(new { message = "Participant profile not found" });

            var enrolments = await _context.EventEnrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.ParticipantId == participant.ParticipantId)
                .OrderByDescending(e => e.EnrolmentDate)
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: /api/events/{eventId}/enrolments (Organiser)
        [HttpGet("events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can view event enrolments" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only view enrolments for your events" });

            var enrolments = await _context.EventEnrolments
                .Include(e => e.Participant).ThenInclude(p => p.User)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .ToListAsync();

            return Ok(enrolments);
        }

        // PUT: /api/enrolments/{enrolmentId}/status
        [HttpPut("enrolments/{enrolmentId}/status")]
        public async Task<IActionResult> UpdateEnrolmentStatus(int enrolmentId, [FromBody] StatusUpdateDto dto)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can update enrolments" });

            var enrolment = await _context.EventEnrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null || enrolment.Event!.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only update your own event enrolments" });

            var validStatuses = new[] { "Pending", "Confirmed", "Cancelled", "Withdrawn", "Completed" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid status" });

            enrolment.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.PaymentStatus))
                enrolment.PaymentStatus = dto.PaymentStatus;
            if (!string.IsNullOrEmpty(dto.BibNumber))
                enrolment.BibNumber = dto.BibNumber;
            if (dto.Notes != null)
                enrolment.Notes = dto.Notes;
            enrolment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(enrolment);
        }

        // DELETE: /api/enrolments/{enrolmentId}
        [HttpDelete("enrolments/{enrolmentId}")]
        public async Task<IActionResult> Withdraw(int enrolmentId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsParticipant(HttpContext))
                return StatusCode(403, new { message = "Only Participants can withdraw" });

            var enrolment = await _context.EventEnrolments.FindAsync(enrolmentId);
            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null || enrolment.ParticipantId != participant.ParticipantId)
                return StatusCode(403, new { message = "You can only withdraw your own enrolment" });

            _context.EventEnrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment cancelled successfully" });
        }
    }

    public class EnrolDto
    {
        public int CategoryId { get; set; }
        public string? Notes { get; set; }
    }

    public class StatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
        public string? PaymentStatus { get; set; }
        public string? BibNumber { get; set; }
        public string? Notes { get; set; }
    }
}