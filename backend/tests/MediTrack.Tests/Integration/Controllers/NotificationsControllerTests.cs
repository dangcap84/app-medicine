using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediTrack.Api;
using MediTrack.Application.Dtos.Auth;
using MediTrack.Application.Dtos.Notification;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MediTrack.Tests.Integration.Controllers
{
    public class NotificationsControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;, IDisposable
    {
        private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;
        private string _authToken;

        public NotificationsControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
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
            // Create test user
            var user = new User 
            { 
                Username = "testuser", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") 
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create test notifications
            var notifications = new List&lt;Notification&gt;
            {
                new Notification 
                { 
                    UserId = user.Id, 
                    Message = "Test Notification 1",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    DueDate = DateTime.UtcNow.AddMinutes(30)
                },
                new Notification 
                { 
                    UserId = user.Id, 
                    Message = "Test Notification 2",
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    DueDate = DateTime.UtcNow.AddMinutes(-30),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-15)
                }
            };
            _context.Notifications.AddRange(notifications);
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
        public async Task GetNotifications_WithoutAuth_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/notifications");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetNotifications_WithAuth_ReturnsAllNotifications()
        {
            // Arrange
            AuthenticateClient();

            // Act
            var response = await _client.GetAsync("/api/notifications");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var notifications = await response.Content.ReadFromJsonAsync&lt;List&lt;NotificationDto&gt;&gt;();
            notifications.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetNotifications_WithAuthAndUnreadOnly_ReturnsUnreadNotifications()
        {
            // Arrange
            AuthenticateClient();

            // Act
            var response = await _client.GetAsync("/api/notifications?onlyUnread=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var notifications = await response.Content.ReadFromJsonAsync&lt;List&lt;NotificationDto&gt;&gt;();
            notifications.Should().HaveCount(1);
            notifications.Should().OnlyContain(n =&gt; !n.IsRead);
        }

        [Fact]
        public async Task MarkAsRead_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var updateDto = new UpdateNotificationDto { IsRead = true };

            // Act
            var response = await _client.PutAsJsonAsync("/api/notifications/1", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task MarkAsRead_WithAuth_ForExistingNotification_ReturnsNoContent()
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateNotificationDto { IsRead = true };

            // Act
            var response = await _client.PutAsJsonAsync("/api/notifications/1", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the notification was updated in the database
            var notification = await _context.Notifications.FindAsync(1);
            notification.IsRead.Should().BeTrue();
            notification.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task MarkAsRead_WithAuth_ForNonexistentNotification_ReturnsNotFound()
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateNotificationDto { IsRead = true };

            // Act
            var response = await _client.PutAsJsonAsync("/api/notifications/999", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task MarkAsRead_WithAuth_WithInvalidId_ReturnsBadRequest(int invalidId)
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateNotificationDto { IsRead = true };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/notifications/{invalidId}", updateDto);

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
