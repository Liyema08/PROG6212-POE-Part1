using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Participant_CanEnrolInEvent()
        {
            var context = GetInMemoryContext();
            var participant = new Participant
            {
                UserId = 1,
                DateOfBirth = DateTime.Now.AddYears(-25),
                Gender = "Male"
            };
            context.Participants.Add(participant);

            var ev = new Event
            {
                OrganiserId = 1,
                Name = "Test Event",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Test",
                Distance = 10,
                EventType = "Run",
                RegistrationDeadline = DateTime.UtcNow.AddDays(20)
            };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var category = new Category
            {
                EventId = ev.EventId,
                Name = "Open",
                CategoryType = "Open"
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var enrolment = new EventEnrolment
            {
                ParticipantId = participant.ParticipantId,
                EventId = ev.EventId,
                CategoryId = category.CategoryId,
                Status = "Pending"
            };

            context.EventEnrolments.Add(enrolment);
            await context.SaveChangesAsync();

            var saved = await context.EventEnrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == participant.ParticipantId);
            Assert.NotNull(saved);
            Assert.Equal("Pending", saved.Status);
        }

        [Fact]
        public async Task Participant_CannotEnrolTwiceInSameEvent()
        {
            var context = GetInMemoryContext();
            var participant = new Participant
            {
                UserId = 1,
                DateOfBirth = DateTime.Now.AddYears(-25),
                Gender = "Male"
            };
            var ev = new Event
            {
                OrganiserId = 1,
                Name = "Test",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Test",
                Distance = 10,
                EventType = "Run",
                RegistrationDeadline = DateTime.UtcNow.AddDays(20)
            };
            context.Participants.Add(participant);
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var category = new Category
            {
                EventId = ev.EventId,
                Name = "Open",
                CategoryType = "Open"
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var enrolment1 = new EventEnrolment
            {
                ParticipantId = participant.ParticipantId,
                EventId = ev.EventId,
                CategoryId = category.CategoryId
            };
            context.EventEnrolments.Add(enrolment1);
            await context.SaveChangesAsync();

            var existing = await context.EventEnrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == participant.ParticipantId
                                       && e.EventId == ev.EventId);

            Assert.NotNull(existing);
        }

        [Fact]
        public async Task Organiser_CanCaptureResult()
        {
            var context = GetInMemoryContext();
            var participant = new Participant { UserId = 1, DateOfBirth = DateTime.Now.AddYears(-25), Gender = "Male" };
            var ev = new Event { OrganiserId = 1, Name = "Test", EventDate = DateTime.UtcNow.AddDays(-1), Location = "Test", Distance = 10, EventType = "Run", RegistrationDeadline = DateTime.UtcNow.AddDays(-10) };
            context.Participants.Add(participant);
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var category = new Category { EventId = ev.EventId, Name = "Open", CategoryType = "Open" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var enrolment = new EventEnrolment
            {
                ParticipantId = participant.ParticipantId,
                EventId = ev.EventId,
                CategoryId = category.CategoryId,
                Status = "Confirmed"
            };
            context.EventEnrolments.Add(enrolment);
            await context.SaveChangesAsync();

            var result = new Result
            {
                EnrolmentId = enrolment.EnrolmentId,
                FinishTime = TimeSpan.FromHours(3) + TimeSpan.FromMinutes(15),
                FinishingPosition = 1,
                OverallPosition = 1,
                Status = "Finished"
            };

            context.Results.Add(result);
            await context.SaveChangesAsync();

            var saved = await context.Results.FirstOrDefaultAsync(r => r.EnrolmentId == enrolment.EnrolmentId);
            Assert.NotNull(saved);
            Assert.Equal("Finished", saved.Status);
        }
    }
}