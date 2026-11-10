using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;
using RaceDay.Contracts;

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
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    Venue = e.Venue,
                    Distance = e.Distance,
                    DistanceUnit = e.DistanceUnit,
                    EventType = e.EventType,
                    BannerImageUrl = e.BannerImageUrl,
                    EntryFee = e.EntryFee,
                    RegistrationDeadline = e.RegistrationDeadline,
                    Status = e.Status,
                    Categories = e.Categories.Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        EventId = c.EventId,
                        Name = c.Name,
                        Description = c.Description,
                        CategoryType = c.CategoryType,
                        MinAge = c.MinAge,
                        MaxAge = c.MaxAge,
                        EntryFee = c.EntryFee
                    }).ToList()
                })
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
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
                return NotFound(new { message = "Event not found" });

            var dto = new EventDto
            {
                EventId = ev.EventId,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                Venue = ev.Venue,
                Distance = ev.Distance,
                DistanceUnit = ev.DistanceUnit,
                EventType = ev.EventType,
                BannerImageUrl = ev.BannerImageUrl,
                EntryFee = ev.EntryFee,
                RegistrationDeadline = ev.RegistrationDeadline,
                Status = ev.Status,
                Categories = ev.Categories.Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    EventId = c.EventId,
                    Name = c.Name,
                    Description = c.Description,
                    CategoryType = c.CategoryType,
                    MinAge = c.MinAge,
                    MaxAge = c.MaxAge,
                    EntryFee = c.EntryFee
                }).ToList()
            };

            return Ok(dto);
        }

        // POST: /api/events
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest dto)
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

            var ev = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                Venue = dto.Venue,
                Distance = dto.Distance,
                DistanceUnit = dto.DistanceUnit,
                EventType = dto.EventType,
                MaxParticipants = dto.MaxParticipants,
                EntryFee = dto.EntryFee,
                RegistrationDeadline = dto.RegistrationDeadline,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            var eventDto = new EventDto
            {
                EventId = ev.EventId,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                Venue = ev.Venue,
                Distance = ev.Distance,
                DistanceUnit = ev.DistanceUnit,
                EventType = ev.EventType,
                BannerImageUrl = ev.BannerImageUrl,
                EntryFee = ev.EntryFee,
                RegistrationDeadline = ev.RegistrationDeadline,
                Status = ev.Status
            };

            return Ok(eventDto);
        }

        // PUT: /api/events/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventRequest updated)
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

            return Ok(new { message = "Updated" });
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
                .Where(e => e.OrganiserId == organiser.OrganiserId)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    Venue = e.Venue,
                    Distance = e.Distance,
                    DistanceUnit = e.DistanceUnit,
                    EventType = e.EventType,
                    BannerImageUrl = e.BannerImageUrl,
                    EntryFee = e.EntryFee,
                    RegistrationDeadline = e.RegistrationDeadline,
                    Status = e.Status,
                    Categories = e.Categories.Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        EventId = c.EventId,
                        Name = c.Name,
                        Description = c.Description,
                        CategoryType = c.CategoryType,
                        MinAge = c.MinAge,
                        MaxAge = c.MaxAge,
                        EntryFee = c.EntryFee
                    }).ToList()
                })
                .ToListAsync();

            return Ok(events);
        }

        // POST: /api/events/{id}/banner
        [HttpPost("/api/events/{id}/banner")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file, [FromServices] RaceDay.API.Services.BlobStorageService blobStorage)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can upload banners" });

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found" });

            string blobName = await blobStorage.UploadAsync(file, "event-banners");
            ev.BannerImageUrl = blobName;
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl = blobName });
        }

        // GET: /api/events/{id}/banner
        [HttpGet("/api/events/{id}/banner")]
        public async Task<IActionResult> GetBanner(int id, [FromServices] RaceDay.API.Services.BlobStorageService blobStorage)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null || string.IsNullOrEmpty(ev.BannerImageUrl)) 
                return NotFound();

            var stream = await blobStorage.DownloadAsync(ev.BannerImageUrl);
            if (stream == null) return NotFound();

            string extension = Path.GetExtension(ev.BannerImageUrl).ToLower();
            string contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream" 
            };

            return File(stream, contentType); 
        }
    }
}