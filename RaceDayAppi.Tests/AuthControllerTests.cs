using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RaceDayApi.Controllers;
using RaceDayApi.DTOs;

namespace RaceDayApi.Tests
{
    public class AuthControllerTests
    {
        private IConfiguration GetConfig()
        {
            var settings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsARaceDayApiSecretKeyThatIsLongEnough123!" },
                { "Jwt:Issuer", "RaceDayApi" },
                { "Jwt:Audience", "RaceDayApiUsers" }
            };

            return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        }

        private RegisterDto NewRegisterDto(string email, string role)
        {
            return new RegisterDto
            {
                Name = "Test",
                Surname = "User",
                Cell = "0712345678",
                Email = email,
                Password = "Password123!",
                Role = role
            };
        }

        [Fact]
        public async Task Register_Organiser_SavesHashedPassword()
        {
            // Arrange
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, GetConfig());

            // Act
            var result = await controller.Register(NewRegisterDto("org@test.com", "Organiser"));

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var saved = context.Organisers.Single();
            Assert.NotEqual("Password123!", saved.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", saved.Password));
        }

        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, GetConfig());

            var result = await controller.Register(NewRegisterDto("admin@test.com", "Admin"));

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Organisers);
            Assert.Empty(context.Participants);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, GetConfig());
            await controller.Register(NewRegisterDto("same@test.com", "Participant"));

            var result = await controller.Register(NewRegisterDto("same@test.com", "Organiser"));

            Assert.IsType<ConflictObjectResult>(result);
            Assert.Empty(context.Organisers);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokenAndRole()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, GetConfig());
            await controller.Register(NewRegisterDto("org@test.com", "Organiser"));

            var result = await controller.Login(new LoginDto { Email = "org@test.com", Password = "Password123!" });

            var ok = Assert.IsType<OkObjectResult>(result);
            var token = ok.Value?.GetType().GetProperty("token")?.GetValue(ok.Value) as string;
            var role = ok.Value?.GetType().GetProperty("role")?.GetValue(ok.Value) as string;

            Assert.False(string.IsNullOrEmpty(token));
            Assert.Equal("Organiser", role);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, GetConfig());
            await controller.Register(NewRegisterDto("org@test.com", "Organiser"));

            var result = await controller.Login(new LoginDto { Email = "org@test.com", Password = "WrongPassword1!" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, GetConfig());

            var result = await controller.Login(new LoginDto { Email = "nobody@test.com", Password = "Password123!" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}