using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EventTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Organiser_CanCreateEvent()
        {
            var context = GetInMemoryContext();
            var user = new User
            {
                Email = "org@test.com",
                PasswordHash = "hash",
                FirstName = "Org",
                LastName = "User",
                Role = "Organiser"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var organiser = new Organiser
            {
                UserId = user.UserId,
                OrganisationName = "Test Org"
            };
            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();

            var ev = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = "Test Event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Test Location",
                Distance = 10,
                EventType = "Run",
                RegistrationDeadline = DateTime.UtcNow.AddDays(20)
            };

            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var saved = await context.Events.FirstOrDefaultAsync(e => e.Name == "Test Event");
            Assert.NotNull(saved);
            Assert.Equal("Test Event", saved.Name);
            Assert.Equal(organiser.OrganiserId, saved.OrganiserId);
        }

        [Fact]
        public async Task Event_ShouldBelongToOrganiser()
        {
            var context = GetInMemoryContext();
            var user = new User
            {
                Email = "org@test.com",
                PasswordHash = "hash",
                FirstName = "Org",
                LastName = "User",
                Role = "Organiser"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var organiser = new Organiser
            {
                UserId = user.UserId,
                OrganisationName = "Test Org"
            };
            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();

            var ev = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = "My Event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Test",
                Distance = 5,
                EventType = "Run",
                RegistrationDeadline = DateTime.UtcNow.AddDays(20)
            };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var saved = await context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.Name == "My Event");

            Assert.NotNull(saved);
            Assert.NotNull(saved.Organiser);
            Assert.Equal("Test Org", saved.Organiser.OrganisationName);
        }

        [Fact]
        public async Task DeleteEvent_ShouldWork_WhenNoEnrolments()
        {
            var context = GetInMemoryContext();
            var ev = new Event
            {
                OrganiserId = 1,
                Name = "Delete Me",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Test",
                Distance = 5,
                EventType = "Run",
                RegistrationDeadline = DateTime.UtcNow.AddDays(20)
            };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            context.Events.Remove(ev);
            await context.SaveChangesAsync();

            var deleted = await context.Events.FirstOrDefaultAsync(e => e.Name == "Delete Me");
            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetAllEvents_ShouldReturnOrderedByDate()
        {
            var context = GetInMemoryContext();
            context.Events.Add(new Event { OrganiserId = 1, Name = "Event 2", EventDate = DateTime.UtcNow.AddDays(20), Location = "B", Distance = 10, EventType = "Run", RegistrationDeadline = DateTime.UtcNow.AddDays(10) });
            context.Events.Add(new Event { OrganiserId = 1, Name = "Event 1", EventDate = DateTime.UtcNow.AddDays(10), Location = "A", Distance = 5, EventType = "Run", RegistrationDeadline = DateTime.UtcNow.AddDays(5) });
            await context.SaveChangesAsync();

            var events = await context.Events.OrderBy(e => e.EventDate).ToListAsync();

            Assert.Equal("Event 1", events[0].Name);
            Assert.Equal("Event 2", events[1].Name);
        }
    }
}