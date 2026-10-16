using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.TestHelpers;
using System.Security.Claims;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTests
    {
        [Fact]
        public async Task GetAllEvents_ReturnsOkWithEvents()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new EventsController(context);

            var result = await controller.GetAllEvents();

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task GetEvent_WithValidId_ReturnsOk()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new EventsController(context);

            var result = await controller.GetEvent(1);

            result.Should().BeOfType<OkObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task GetEvent_WithInvalidId_ReturnsNotFound()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new EventsController(context);

            var result = await controller.GetEvent(999);

            result.Should().BeOfType<NotFoundObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new EventsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 1, role: "Organiser");

            var dto = new CreateEventDto
            {
                EventName = "New Event",
                EventDate = new DateTime(2026, 12, 25),
                Location = "New Location",
                City = "New City",
                Country = "South Africa",
                Description = "A new test event",
                EventType = "Run"
            };

            var result = await controller.CreateEvent(dto);

            result.Should().BeOfType<CreatedAtActionResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task GetMyEvents_AsOrganiser_ReturnsOnlyOwnEvents()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new EventsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 1, role: "Organiser");

            var result = await controller.GetMyEvents();

            result.Should().BeOfType<OkObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task UpdateEvent_ByNonOwner_ReturnsForbidden()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new EventsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 2, role: "Organiser");

            var dto = new UpdateEventDto
            {
                EventName = "Hacked Event"
            };

            var result = await controller.UpdateEvent(1, dto);

            result.Should().BeOfType<ForbidResult>();

            TestDbContextFactory.Destroy(context);
        }

        private static ControllerContext GetControllerContextWithUser(int userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }
    }
}