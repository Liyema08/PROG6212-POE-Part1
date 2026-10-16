using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: /api/events
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            var events = await _context.Events
                .Include(e => e.Categories)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events);
        }

        // GET: /api/events/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            var ev = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
                return NotFound(new { message = "Event not found" });

            return Ok(ev);
        }

        // POST: /api/events
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event ev)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can create events" });

            var userId = SessionHelper.GetUserId(HttpContext);

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return BadRequest(new { message = "Organiser profile not found" });

            ev.OrganiserId = organiser.OrganiserId;
            ev.CreatedAt = DateTime.UtcNow;

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, ev);
        }

        // PUT: /api/events/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updated)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can update events" });

            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only update your own events" });

            ev.Name = updated.Name;
            ev.Description = updated.Description;
            ev.EventDate = updated.EventDate;
            ev.Location = updated.Location;
            ev.Venue = updated.Venue;
            ev.Distance = updated.Distance;
            ev.DistanceUnit = updated.DistanceUnit;
            ev.EventType = updated.EventType;
            ev.MaxParticipants = updated.MaxParticipants;
            ev.EntryFee = updated.EntryFee;
            ev.RegistrationDeadline = updated.RegistrationDeadline;
            ev.Status = updated.Status;
            ev.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ev);
        }

        // DELETE: /api/events/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can delete events" });

            var ev = await _context.Events
                .Include(e => e.Enrolments)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
                return NotFound(new { message = "Event not found" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only delete your own events" });

            if (ev.Enrolments.Any())
                return Conflict(new { message = "Cannot delete event with enrolments" });

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully" });
        }

        // GET: /api/events/organiser
        [HttpGet("organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can access this" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return BadRequest(new { message = "Organiser profile not found" });

            var events = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.Enrolments)
                .Where(e => e.OrganiserId == organiser.OrganiserId)
                .ToListAsync();

            return Ok(events);
        }
    }
}