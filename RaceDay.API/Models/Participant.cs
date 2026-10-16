using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EmergencyContact { get; set; }

        [StringLength(20)]
        public string? EmergencyPhone { get; set; }

        [StringLength(500)]
        public string? MedicalConditions { get; set; }

        [StringLength(50)]
        public string? EmergencyContactRelation { get; set; }

        [StringLength(100)]
        public string? ClubName { get; set; }

        [StringLength(50)]
        public string? LicenseNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User? User { get; set; }
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}