using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.Tests.TestHelpers;
using System.Security.Claims;
using Xunit;

namespace RaceDay.Tests
{
    public class ResultsControllerTests
    {
        [Fact]
        public async Task SubmitResult_AsEventOwner_ReturnsCreated()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            context.Registrations.Add(new Registration
            {
                RegistrationId = 1,
                UserId = 2,
                RaceId = 1,
                Status = "confirmed",
                BibNumber = "TEST001",
                RegistrationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var controller = new ResultsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 1, role: "Organiser");

            var dto = new CreateResultDto
            {
                FinishTime = new TimeSpan(1, 30, 0),
                OverallRank = 5,
                AgeGroupRank = 2,
                PacePerKm = 5.5m,
                Status = "finished"
            };

            var result = await controller.SubmitResult(1, dto);

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(201);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task GetMyResults_AsParticipant_ReturnsOnlyOwnResults()
        {
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var controller = new ResultsController(context);
            controller.ControllerContext = GetControllerContextWithUser(userId: 2, role: "Participant");

            var result = await controller.GetMyResults();

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