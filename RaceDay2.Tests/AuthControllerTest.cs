
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay2.API.Controllers;
using RaceDay2.API.Data;
using Xunit;

namespace RaceDay2.Tests
{
    public class AuthControllerTests
    {
        private readonly RaceDayDbContext _context;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "RaceDay2-Development-Key-2026-Secure-Only-For-Local-Testing!",
                ["Jwt:Issuer"] = "RaceDay2.API",
                ["Jwt:Audience"] = "RaceDay2.Client"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _controller = new AuthController(_context, configuration);
        }

        [Fact]
        public async Task Register_NewUser_ReturnsSuccess()
        {
            var request = new AuthController.RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                Password = "TestPassword123",
                PhoneNumber = "0712345678"
            };

            var result = await _controller.Register(request);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(1, await _context.Users.CountAsync());
        }

        [Fact]
        public async Task Register_ExistingEmail_ReturnsConflict()
        {
            var request = new AuthController.RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                Password = "TestPassword123"
            };

            await _controller.Register(request);

            var result = await _controller.Register(request);

            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Equal(1, await _context.Users.CountAsync());
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var request = new AuthController.LoginRequest
            {
                Email = "unknown@example.com",
                Password = "WrongPassword123"
            };

            var result = await _controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var registerRequest = new AuthController.RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@example.com",
                Password = "TestPassword123"
            };

            await _controller.Register(registerRequest);

            var loginRequest = new AuthController.LoginRequest
            {
                Email = "participant@example.com",
                Password = "TestPassword123"
            };

            var result = await _controller.Login(loginRequest);

            var response = Assert.IsType<AuthController.AuthResponse>(
                Assert.IsType<OkObjectResult>(result.Result).Value);

            Assert.False(string.IsNullOrWhiteSpace(response.Token));
            Assert.Equal("Participant", response.Role);
        }
    }
}
