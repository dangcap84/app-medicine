using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediTrack.Api;
using MediTrack.Application.Dtos.Auth;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace MediTrack.Tests.Integration.Controllers
{
    public class AuthControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;, IDisposable
    {
        private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;

        public AuthControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService&lt;ApplicationDbContext&gt;();

            // Setup test data
            SetupTestData().Wait();
        }

        private async Task SetupTestData()
        {
            // Create existing user for testing duplicate registration
            var existingUser = new User 
            { 
                Username = "existinguser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsOkWithToken()
        {
            // Arrange
            var registerDto = new RegisterRequestDto 
            { 
                Username = "newuser",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync&lt;AuthResponseDto&gt;();
            result.Token.Should().NotBeNullOrEmpty();

            // Verify user was created in database
            var user = await _context.Users.FindAsync(result.UserId);
            user.Should().NotBeNull();
            user.Username.Should().Be(registerDto.Username);
        }

        [Fact]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterRequestDto 
            { 
                Username = "existinguser",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Username already exists");
        }

        [Theory]
        [InlineData("", "password123")]
        [InlineData(" ", "password123")]
        [InlineData(null, "password123")]
        [InlineData("newuser", "")]
        [InlineData("newuser", " ")]
        [InlineData("newuser", null)]
        public async Task Register_WithInvalidData_ReturnsBadRequest(string username, string password)
        {
            // Arrange
            var registerDto = new RegisterRequestDto 
            { 
                Username = username,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginRequestDto 
            { 
                Username = "existinguser",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync&lt;AuthResponseDto&gt;();
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto 
            { 
                Username = "nonexistentuser",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto 
            { 
                Username = "existinguser",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("", "password123")]
        [InlineData(" ", "password123")]
        [InlineData(null, "password123")]
        [InlineData("existinguser", "")]
        [InlineData("existinguser", " ")]
        [InlineData("existinguser", null)]
        public async Task Login_WithInvalidData_ReturnsBadRequest(string username, string password)
        {
            // Arrange
            var loginDto = new LoginRequestDto 
            { 
                Username = username,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        public void Dispose()
        {
            // Cleanup
            _context.Database.EnsureDeleted();
            _scope.Dispose();
        }
    }
}
