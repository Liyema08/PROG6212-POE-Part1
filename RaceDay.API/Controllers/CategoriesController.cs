using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: /api/events/{eventId}/categories
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: /api/events/{eventId}/categories
        [HttpPost("events/{eventId}/categories")]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] Category category)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can create categories" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found" });

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only add categories to your own events" });

            category.EventId = eventId;
            category.CreatedAt = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { eventId = eventId }, category);
        }

        // PUT: /api/categories/{id}
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updated)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can update categories" });

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null || category.Event!.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only update your own event categories" });

            category.Name = updated.Name;
            category.Description = updated.Description;
            category.CategoryType = updated.CategoryType;
            category.MinAge = updated.MinAge;
            category.MaxAge = updated.MaxAge;
            category.MinDistance = updated.MinDistance;
            category.MaxDistance = updated.MaxDistance;
            category.GenderRestriction = updated.GenderRestriction;
            category.EntryFee = updated.EntryFee;
            category.MaxEntries = updated.MaxEntries;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // DELETE: /api/categories/{id}
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            if (!SessionHelper.IsOrganiser(HttpContext))
                return StatusCode(403, new { message = "Only Organisers can delete categories" });

            var category = await _context.Categories
                .Include(c => c.Event)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null || category.Event!.OrganiserId != organiser.OrganiserId)
                return StatusCode(403, new { message = "You can only delete your own event categories" });

            if (category.Enrolments.Any())
                return Conflict(new { message = "Cannot delete category with enrolments" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully" });
        }
    }
}