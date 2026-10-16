using Microsoft.EntityFrameworkCore;  
using RaceDay.API.Models;              
using System;                          
using System.Collections.Generic;      
using System.Threading.Tasks;          
namespace RaceDay.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedData(RaceDayDbContext context)
        {
            // Check if data already exists
            if (context.Users.Any())
                return;

            // Seed Users
            var users = new List<User>
            {
                new User
                {
                    Email = "sarah.johnson@runco.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Sarah Johnson",
                    DateOfBirth = new DateTime(1985, 3, 15),
                    Gender = "F",
                    Phone = "+27-31-555-0101",
                    EmergencyContact = "Mike Johnson - +27-82-555-0102",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "michael.chen@raceplus.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Michael Chen",
                    DateOfBirth = new DateTime(1978, 11, 22),
                    Gender = "M",
                    Phone = "+27-21-555-0201",
                    EmergencyContact = "Lisa Chen - +27-83-555-0202",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "emma.wilson@email.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Emma Wilson",
                    DateOfBirth = new DateTime(1992, 7, 8),
                    Gender = "F",
                    Phone = "+27-82-555-0301",
                    EmergencyContact = "David Wilson - +27-82-555-0302",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "james.rodriguez@email.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "James Rodriguez",
                    DateOfBirth = new DateTime(1988, 9, 23),
                    Gender = "M",
                    Phone = "+27-83-555-0401",
                    EmergencyContact = "Anna Rodriguez - +27-83-555-0402",
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // Seed Events
            var events = new List<Event>
            {
                new Event
                {
                    OrganizerId = 1,
                    EventName = "Comrades Marathon 2026",
                    EventDate = new DateTime(2026, 6, 14),
                    Location = "Pietermaritzburg to Durban",
                    City = "Durban",
                    Country = "South Africa",
                    Description = "The Ultimate Human Race - a 90km ultramarathon between Pietermaritzburg and Durban.",
                    EventType = "Run",
                    CreatedAt = DateTime.UtcNow
                },
                new Event
                {
                    OrganizerId = 2,
                    EventName = "Two Oceans Marathon 2026",
                    EventDate = new DateTime(2026, 4, 4),
                    Location = "Cape Peninsula",
                    City = "Cape Town",
                    Country = "South Africa",
                    Description = "The world's most beautiful marathon - 56km ultra and 21km half marathon.",
                    EventType = "Run",
                    CreatedAt = DateTime.UtcNow
                },
                new Event
                {
                    OrganizerId = 1,
                    EventName = "Soweto Marathon 2026",
                    EventDate = new DateTime(2026, 11, 1),
                    Location = "Soweto",
                    City = "Johannesburg",
                    Country = "South Africa",
                    Description = "The People's Race - a 42km marathon through the historic streets of Soweto.",
                    EventType = "Run",
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Events.AddRange(events);
            await context.SaveChangesAsync();

            // Seed Races
            var races = new List<Race>
            {
                new Race
                {
                    EventId = 1,
                    RaceName = "Comrades Ultra Marathon",
                    DistanceKm = 90.0m,
                    StartTime = new DateTime(2026, 6, 14, 5, 30, 0),
                    EndTime = new DateTime(2026, 6, 14, 17, 30, 0),
                    AgeGroup = "20+",
                    MaxParticipants = 25000,
                    CreatedAt = DateTime.UtcNow
                },
                new Race
                {
                    EventId = 2,
                    RaceName = "Two Oceans Ultra 56km",
                    DistanceKm = 56.0m,
                    StartTime = new DateTime(2026, 4, 4, 6, 0, 0),
                    EndTime = new DateTime(2026, 4, 4, 13, 0, 0),
                    AgeGroup = "20+",
                    MaxParticipants = 14000,
                    CreatedAt = DateTime.UtcNow
                },
                new Race
                {
                    EventId = 2,
                    RaceName = "Two Oceans Half Marathon 21km",
                    DistanceKm = 21.1m,
                    StartTime = new DateTime(2026, 4, 4, 6, 30, 0),
                    EndTime = new DateTime(2026, 4, 4, 11, 0, 0),
                    AgeGroup = "18+",
                    MaxParticipants = 16000,
                    CreatedAt = DateTime.UtcNow
                },
                new Race
                {
                    EventId = 3,
                    RaceName = "Soweto Marathon 42km",
                    DistanceKm = 42.2m,
                    StartTime = new DateTime(2026, 11, 1, 6, 0, 0),
                    EndTime = new DateTime(2026, 11, 1, 12, 0, 0),
                    AgeGroup = "20+",
                    MaxParticipants = 12000,
                    CreatedAt = DateTime.UtcNow
                },
                new Race
                {
                    EventId = 3,
                    RaceName = "Soweto Half Marathon 21km",
                    DistanceKm = 21.1m,
                    StartTime = new DateTime(2026, 11, 1, 6, 30, 0),
                    EndTime = new DateTime(2026, 11, 1, 10, 30, 0),
                    AgeGroup = "18+",
                    MaxParticipants = 8000,
                    CreatedAt = DateTime.UtcNow
                },
                new Race
                {
                    EventId = 3,
                    RaceName = "Soweto 10km Fun Run",
                    DistanceKm = 10.0m,
                    StartTime = new DateTime(2026, 11, 1, 7, 0, 0),
                    EndTime = new DateTime(2026, 11, 1, 9, 30, 0),
                    AgeGroup = "All Ages",
                    MaxParticipants = 5000,
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Races.AddRange(races);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Database seeded successfully with SA event data!");
        }
    }
}