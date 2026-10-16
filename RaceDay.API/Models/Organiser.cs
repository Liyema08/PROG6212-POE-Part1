using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Organiser
    {
        [Key]
        public int OrganiserId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string OrganisationName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? OrganisationType { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        [StringLength(200)]
        public string? WebsiteUrl { get; set; }

        public bool IsVerified { get; set; } = false;
        public DateTime? VerificationDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User? User { get; set; }
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}