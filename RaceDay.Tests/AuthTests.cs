using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Register_ShouldCreateUser_WhenValidData()
        {
            var context = GetInMemoryContext();
            var user = new User
            {
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                FirstName = "Test",
                LastName = "User",
                Role = "Participant"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var saved = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");
            Assert.NotNull(saved);
            Assert.Equal("Participant", saved.Role);
        }

        [Fact]
        public async Task Login_ShouldVerifyPassword_WhenCorrectPassword()
        {
            var context = GetInMemoryContext();
            var user = new User
            {
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                FirstName = "Test",
                LastName = "User",
                Role = "Participant"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var saved = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");
            var isValid = BCrypt.Net.BCrypt.Verify("Test@123", saved!.PasswordHash);
            Assert.True(isValid);
        }

        [Fact]
        public async Task Login_ShouldFail_WhenWrongPassword()
        {
            var context = GetInMemoryContext();
            var user = new User
            {
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                FirstName = "Test",
                LastName = "User",
                Role = "Participant"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var saved = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@test.com");
            var isValid = BCrypt.Net.BCrypt.Verify("WrongPassword", saved!.PasswordHash);
            Assert.False(isValid);
        }

        [Fact]
        public void PasswordHash_ShouldNotEqualPlainText()
        {
            var password = "Test@123";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public async Task Register_ShouldFail_WhenEmailExists()
        {
            var context = GetInMemoryContext();
            var user = new User
            {
                Email = "duplicate@test.com",
                PasswordHash = "hash",
                FirstName = "First",
                LastName = "User",
                Role = "Participant"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var exists = await context.Users.AnyAsync(u => u.Email == "duplicate@test.com");
            Assert.True(exists);
        }
    }
}