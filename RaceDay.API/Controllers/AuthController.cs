using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email exists
            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                return Conflict(new { message = "Email already exists" });

            if (dto.Role != "Organiser" && dto.Role != "Participant")
                return BadRequest(new { message = "Role must be Organiser or Participant" });

            // Create user
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create Organiser or Participant profile
            if (dto.Role == "Organiser")
            {
                var organiser = new Organiser
                {
                    UserId = user.UserId,
                    OrganisationName = dto.OrganisationName ?? "N/A",
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Organisers.Add(organiser);
            }
            else
            {
                var participant = new Participant
                {
                    UserId = user.UserId,
                    DateOfBirth = dto.DateOfBirth ?? DateTime.Now.AddYears(-20),
                    Gender = dto.Gender ?? "Other",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new
            {
                userId = user.UserId,
                email = user.Email,
                role = user.Role,
                message = "Registration successful"
            });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            if (!user.IsActive)
                return Unauthorized(new { message = "Account is inactive" });

            // Store session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserEmail", user.Email);

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                role = user.Role,
                message = "Login successful"
            });
        }

        // POST: /api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }

        // GET: /api/auth/me
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Not logged in" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            return Ok(new
            {
                userId = user.UserId,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role,
                profilePictureUrl = user.ProfilePictureUrl
            });
        }
    }

    // DTOs
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        public string? OrganisationName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}