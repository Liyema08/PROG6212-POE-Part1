using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: /api/events/{eventId}/results
        [HttpPost("events/{eventId}/results")]
        public async Task<IActionResult> CaptureResult(int eventId, [FromBody] ResultDto dto)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can capture results" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only capture results for your events" });

            var enrolment = await _context.EventEnrolments.FindAsync(dto.EnrolmentId);
            if (enrolment == null || enrolment.EventId != eventId)
                return NotFound(new { message = "Enrolment not found for this event" });

            var existing = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentId == dto.EnrolmentId);

            if (existing != null)
                return Conflict(new { message = "Result already exists for this enrolment" });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                FinishingPosition = dto.FinishingPosition,
                OverallPosition = dto.OverallPosition,
                GenderPosition = dto.GenderPosition,
                AgeGroupPosition = dto.AgeGroupPosition,
                Status = dto.Status ?? "Finished",
                CreatedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyResults), new { id = result.ResultId }, result);
        }

        // GET: /api/results/me
        [HttpGet("results/me")]
        public async Task<IActionResult> GetMyResults()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsParticipant(HttpContext))
                return StatusCode(403, new { message = "Only Participants can view their results" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return BadRequest(new { message = "Participant profile not found" });

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.ParticipantId == participant.ParticipantId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(results);
        }

        // GET: /api/events/{eventId}/results
        [HttpGet("events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Participant).ThenInclude(p => p!.User)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.EventId == eventId)
                .OrderBy(r => r.OverallPosition)
                .ToListAsync();

            return Ok(results);
        }

        // GET: /api/results/{id}
        [HttpGet("results/{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Participant).ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found" });

            return Ok(result);
        }

        // PUT: /api/results/{id}
        [HttpPut("results/{id}")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultDto dto)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can update results" });

            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null || result.Enrolment!.Event!.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only update your own event results" });

            result.FinishTime = dto.FinishTime;
            result.FinishingPosition = dto.FinishingPosition;
            result.OverallPosition = dto.OverallPosition;
            result.GenderPosition = dto.GenderPosition;
            result.AgeGroupPosition = dto.AgeGroupPosition;
            result.Status = dto.Status ?? result.Status;
            result.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(result);
        }

        // DELETE: /api/results/{id}
        [HttpDelete("results/{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can delete results" });

            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null || result.Enrolment!.Event!.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only delete your own event results" });

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Result deleted successfully" });
        }
    }

    public class ResultDto
    {
        public int EnrolmentId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int FinishingPosition { get; set; }
        public int OverallPosition { get; set; }
        public int? GenderPosition { get; set; }
        public int? AgeGroupPosition { get; set; }
        public string? Status { get; set; }
    }
}