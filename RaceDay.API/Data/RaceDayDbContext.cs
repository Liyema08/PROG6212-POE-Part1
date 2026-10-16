using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventEnrolment> EventEnrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // USER - Email must be unique
            // ============================================
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ============================================
            // USER → ORGANISER (1:1)
            // ============================================
            modelBuilder.Entity<Organiser>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // USER → PARTICIPANT (1:1)
            // ============================================
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // ORGANISER → EVENT (1:M) — Restrict to avoid cycle
            // ============================================
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // EVENT → CATEGORY (1:M) — Restrict to avoid cycle
            // ============================================
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // PARTICIPANT → EVENTENROLMENT (1:M) — Restrict
            // ============================================
            modelBuilder.Entity<EventEnrolment>()
                .HasOne(ee => ee.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(ee => ee.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // EVENT → EVENTENROLMENT (1:M) — Restrict
            // ============================================
            modelBuilder.Entity<EventEnrolment>()
                .HasOne(ee => ee.Event)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(ee => ee.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // CATEGORY → EVENTENROLMENT (1:M) — Restrict
            // ============================================
            modelBuilder.Entity<EventEnrolment>()
                .HasOne(ee => ee.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(ee => ee.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // EVENTENROLMENT → RESULT (1:1) — Cascade OK
            // ============================================
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(ee => ee.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // UNIQUE: Participant can only enrol ONCE per event
            // ============================================
            modelBuilder.Entity<EventEnrolment>()
                .HasIndex(ee => new { ee.ParticipantId, ee.EventId })
                .IsUnique();
        }
    }
}