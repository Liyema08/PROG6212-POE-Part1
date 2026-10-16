using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string CategoryType { get; set; } = string.Empty;

        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public decimal? MinDistance { get; set; }
        public decimal? MaxDistance { get; set; }

        [StringLength(20)]
        public string? GenderRestriction { get; set; }

        public decimal? EntryFee { get; set; }
        public int? MaxEntries { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("EventId")]
        public Event? Event { get; set; }
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}