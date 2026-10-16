using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Venue { get; set; }

        [Required]
        public decimal Distance { get; set; }

        [StringLength(10)]
        public string DistanceUnit { get; set; } = "km";

        [Required]
        [StringLength(20)]
        public string EventType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? BannerImageUrl { get; set; }

        public int MaxParticipants { get; set; } = 100;
        public decimal EntryFee { get; set; } = 0;

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Upcoming";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("OrganiserId")]
        public Organiser? Organiser { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}