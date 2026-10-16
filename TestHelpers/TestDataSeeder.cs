using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.Tests.TestHelpers
{
    public static class TestDataSeeder
    {
        public static void SeedTestData(RaceDayDbContext context)
        {
            // Seed Users
            var users = new List<User>
            {
                new User
                {
                    UserId = 1,
                    Email = "organiser@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Test Organiser",
                    Gender = "M",
                    Phone = "+27-82-111-1111",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 2,
                    Email = "participant@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Test Participant",
                    Gender = "F",
                    Phone = "+27-82-222-2222",
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // Seed Events
            var events = new List<Event>
            {
                new Event
                {
                    EventId = 1,
                    OrganizerId = 1,
                    EventName = "Test Marathon 2026",
                    EventDate = new DateTime(2026, 6, 14),
                    Location = "Test Location",
                    City = "Test City",
                    Country = "South Africa",
                    Description = "Test event",
                    EventType = "Run",
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Events.AddRange(events);
            context.SaveChanges();

            // Seed Races
            var races = new List<Race>
            {
                new Race
                {
                    RaceId = 1,
                    EventId = 1,
                    RaceName = "Test 10km",
                    DistanceKm = 10.0m,
                    StartTime = new DateTime(2026, 6, 14, 7, 0, 0),
                    EndTime = new DateTime(2026, 6, 14, 10, 0, 0),
                    AgeGroup = "18+",
                    MaxParticipants = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new Race
                {
                    RaceId = 2,
                    EventId = 1,
                    RaceName = "Test 21km",
                    DistanceKm = 21.1m,
                    StartTime = new DateTime(2026, 6, 14, 6, 0, 0),
                    EndTime = new DateTime(2026, 6, 14, 11, 0, 0),
                    AgeGroup = "18+",
                    MaxParticipants = 50,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Races.AddRange(races);
            context.SaveChanges();
        }
    }
}