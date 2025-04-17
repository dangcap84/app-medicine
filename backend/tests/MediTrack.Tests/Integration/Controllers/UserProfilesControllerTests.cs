using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediTrack.Api;
using MediTrack.Application.Dtos.Auth;
using MediTrack.Application.Dtos.UserProfile;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace MediTrack.Tests.Integration.Controllers
{
    public class UserProfilesControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;, IDisposable
    {
        private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;
        private string _authToken;
        private int _userId;

        public UserProfilesControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
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
            // Create test user and profile
            var user = new User 
            { 
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _userId = user.Id;

            var userProfile = new UserProfile
            {
                UserId = user.Id,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };
            _context.UserProfiles.Add(userProfile);
            await _context.SaveChangesAsync();

            // Get auth token
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto 
            { 
                Username = "testuser",
                Password = "password123"
            });
            var authResult = await loginResponse.Content.ReadFromJsonAsync&lt;AuthResponseDto&gt;();
            _authToken = authResult.Token;
        }

        private void AuthenticateClient()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }

        [Fact]
        public async Task GetUserProfile_WithoutAuth_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/userprofiles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUserProfile_WithAuth_ReturnsUserProfile()
        {
            // Arrange
            AuthenticateClient();

            // Act
            var response = await _client.GetAsync("/api/userprofiles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var profile = await response.Content.ReadFromJsonAsync&lt;UserProfileDto&gt;();
            profile.Should().NotBeNull();
            profile.UserId.Should().Be(_userId);
            profile.FirstName.Should().Be("John");
            profile.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task UpdateUserProfile_WithAuth_WithValidData_ReturnsNoContent()
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "John Updated",
                LastName = "Doe Updated",
                DateOfBirth = new DateTime(1991, 2, 2),
                Gender = "Male"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/userprofiles", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify profile was updated
            var updatedProfile = await _context.UserProfiles.FindAsync(_userId);
            updatedProfile.FirstName.Should().Be(updateDto.FirstName);
            updatedProfile.LastName.Should().Be(updateDto.LastName);
            updatedProfile.DateOfBirth.Should().Be(updateDto.DateOfBirth);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateUserProfile_WithAuth_WithInvalidFirstName_ReturnsBadRequest(string invalidFirstName)
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = invalidFirstName,
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/userprofiles", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateUserProfile_WithAuth_WithInvalidLastName_ReturnsBadRequest(string invalidLastName)
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = invalidLastName,
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/userprofiles", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateUserProfile_WithAuth_WithFutureDateOfBirth_ReturnsBadRequest()
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.UtcNow.AddDays(1),
                Gender = "Male"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/userprofiles", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("InvalidGender")]
        public async Task UpdateUserProfile_WithAuth_WithInvalidGender_ReturnsBadRequest(string invalidGender)
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = invalidGender
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/userprofiles", updateDto);

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
