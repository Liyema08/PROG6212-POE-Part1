using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: /api/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phone = user.Phone,
                role = user.Role,
                profilePictureUrl = user.ProfilePictureUrl,
                organisation = user.Organiser != null ? new
                {
                    user.Organiser.OrganisationName,
                    user.Organiser.OrganisationType,
                    user.Organiser.Bio,
                    user.Organiser.WebsiteUrl
                } : null,
                participant = user.Participant != null ? new
                {
                    user.Participant.DateOfBirth,
                    user.Participant.Gender,
                    user.Participant.EmergencyContact,
                    user.Participant.EmergencyPhone,
                    user.Participant.ClubName
                } : null
            });
        }

        // PUT: /api/users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            var userId = SessionHelper.GetUserId(HttpContext);
            var user = await _context.Users
                .Include(u => u.Organiser)
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update base fields
            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
            if (dto.Phone != null) user.Phone = dto.Phone;

            // Role-specific updates
            if (user.Role == "Organiser" && user.Organiser != null)
            {
                if (!string.IsNullOrEmpty(dto.OrganisationName))
                    user.Organiser.OrganisationName = dto.OrganisationName;
                if (dto.Bio != null) user.Organiser.Bio = dto.Bio;
                if (dto.WebsiteUrl != null) user.Organiser.WebsiteUrl = dto.WebsiteUrl;
            }
            else if (user.Role == "Participant" && user.Participant != null)
            {
                if (dto.EmergencyContact != null) user.Participant.EmergencyContact = dto.EmergencyContact;
                if (dto.EmergencyPhone != null) user.Participant.EmergencyPhone = dto.EmergencyPhone;
                if (dto.ClubName != null) user.Participant.ClubName = dto.ClubName;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }

        // GET: /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublicProfile(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return Unauthorized(new { message = "Please log in" });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                userId = user.UserId,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role,
                profilePictureUrl = user.ProfilePictureUrl
            });
        }
    }

    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        // Organiser
        public string? OrganisationName { get; set; }
        public string? Bio { get; set; }
        public string? WebsiteUrl { get; set; }
        // Participant
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? ClubName { get; set; }
    }
}