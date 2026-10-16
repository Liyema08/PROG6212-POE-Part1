using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int FinishingPosition { get; set; }

        [Required]
        public int OverallPosition { get; set; }

        public int? GenderPosition { get; set; }
        public int? AgeGroupPosition { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Finished";

        [StringLength(500)]
        public string? CertificateUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("EnrolmentId")]
        public EventEnrolment? Enrolment { get; set; }
    }
}