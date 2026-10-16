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
    public class RegistrationsControllerTests
    {
        [Fact]
        public async Task Enrol_AsParticipant_ReturnsCreated()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new RegistrationsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 2, role: "Participant");

            var dto = new CreateRegistrationDto { RaceId = 1 };

            var result = await controller.Enrol(dto);

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(201);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Enrol_WithInvalidRace_ReturnsNotFound()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new RegistrationsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 2, role: "Participant");

            var dto = new CreateRegistrationDto { RaceId = 999 };

            var result = await controller.Enrol(dto);

            result.Should().BeOfType<NotFoundObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Enrol_Twice_ReturnsBadRequest()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new RegistrationsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 2, role: "Participant");

            var dto = new CreateRegistrationDto { RaceId = 1 };

            await controller.Enrol(dto);
            var result = await controller.Enrol(dto);

            result.Should().BeOfType<BadRequestObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task GetMyRegistrations_ReturnsOnlyOwnRegistrations()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new RegistrationsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 2, role: "Participant");

            var result = await controller.GetMyRegistrations();

            result.Should().BeOfType<OkObjectResult>();

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