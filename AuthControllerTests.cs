using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Services;
using RaceDay.Tests.TestHelpers;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var jwtService = new JwtService(GetTestConfiguration());
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var dto = new RegisterDto
            {
                Email = "newuser@test.com",
                Password = "Password123!",
                FullName = "New User",
                Role = "Participant",
                Gender = "M",
                Phone = "+27-82-123-4567"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(201);

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var jwtService = new JwtService(GetTestConfiguration());
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var dto = new RegisterDto
            {
                Email = "organiser@test.com",
                Password = "Password123!",
                FullName = "Duplicate User",
                Role = "Participant"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var jwtService = new JwtService(GetTestConfiguration());
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var dto = new RegisterDto
            {
                Email = "newuser@test.com",
                Password = "Password123!",
                FullName = "New User",
                Role = "InvalidRole"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var jwtService = new JwtService(GetTestConfiguration());
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var dto = new LoginDto
            {
                Email = "organiser@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await controller.Login(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as AuthResponseDto;
            response!.Token.Should().NotBeNullOrEmpty();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            TestDataSeeder.SeedTestData(context);

            var jwtService = new JwtService(GetTestConfiguration());
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var dto = new LoginDto
            {
                Email = "organiser@test.com",
                Password = "WrongPassword!"
            };

            // Act
            var result = await controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var jwtService = new JwtService(GetTestConfiguration());
            var authService = new AuthService(context, jwtService);
            var controller = new AuthController(authService);

            var dto = new LoginDto
            {
                Email = "nonexistent@test.com",
                Password = "Password123!"
            };

            // Act
            var result = await controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();

            TestDbContextFactory.Destroy(context);
        }

        private static IConfiguration GetTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", "ThisIsATestKeyForJWTTokenGenerationForTestingPurposes123!@#"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }
    }
}