using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediTrack.Api;
using MediTrack.Application.Dtos.Auth;
using MediTrack.Application.Dtos.Schedule;
using MediTrack.Domain.Entities;
using MediTrack.Domain.Enums;
using MediTrack.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

namespace MediTrack.Tests.Integration.Controllers
{
    public class SchedulesControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;, IDisposable
    {
        private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;
        private string _authToken;
        private int _userId;
        private int _medicineId;

        public SchedulesControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
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
            _userId = user.Id;

            // Create test medicine
            var medicine = new Medicine
            {
                UserId = user.Id,
                Name = "Test Medicine",
                Dosage = "10mg",
                StartedDate = DateTime.UtcNow.AddDays(-5)
            };
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();
            _medicineId = medicine.Id;

            // Create test schedules
            var scheduleTimes = new List&lt;ScheduleTime&gt;
            {
                new ScheduleTime { Time = new TimeSpan(9, 0, 0) },
                new ScheduleTime { Time = new TimeSpan(21, 0, 0) }
            };

            var schedules = new List&lt;Schedule&gt;
            {
                new Schedule 
                { 
                    UserId = user.Id,
                    MedicineId = medicine.Id,
                    FrequencyType = FrequencyType.Daily,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddDays(25),
                    ScheduleTimes = scheduleTimes
                }
            };
            _context.Schedules.AddRange(schedules);
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
        public async Task GetSchedules_WithoutAuth_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/schedules");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSchedules_WithAuth_ReturnsAllSchedulesForUser()
        {
            // Arrange
            AuthenticateClient();

            // Act
            var response = await _client.GetAsync("/api/schedules");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var schedules = await response.Content.ReadFromJsonAsync&lt;List&lt;ScheduleDto&gt;&gt;();
            schedules.Should().HaveCount(1);
            schedules.Should().OnlyContain(s =&gt; s.UserId == _userId);
            schedules[0].ScheduleTimes.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSchedule_WithAuth_ForExistingSchedule_ReturnsSchedule()
        {
            // Arrange
            AuthenticateClient();
            var existingSchedule = await _context.Schedules.FindAsync(1);

            // Act
            var response = await _client.GetAsync($"/api/schedules/{existingSchedule.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var schedule = await response.Content.ReadFromJsonAsync&lt;ScheduleDto&gt;();
            schedule.Should().NotBeNull();
            schedule.MedicineId.Should().Be(existingSchedule.MedicineId);
            schedule.FrequencyType.Should().Be(existingSchedule.FrequencyType);
        }

        [Fact]
        public async Task CreateSchedule_WithAuth_WithValidData_ReturnsCreatedSchedule()
        {
            // Arrange
            AuthenticateClient();
            var createDto = new CreateScheduleDto
            {
                MedicineId = _medicineId,
                FrequencyType = FrequencyType.SpecificDays,
                SpecificDays = "Monday,Wednesday,Friday",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Times = new List&lt;CreateScheduleTimeDto&gt;
                {
                    new CreateScheduleTimeDto { Time = new TimeSpan(8, 0, 0) },
                    new CreateScheduleTimeDto { Time = new TimeSpan(20, 0, 0) }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/schedules", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var schedule = await response.Content.ReadFromJsonAsync&lt;ScheduleDto&gt;();
            schedule.Should().NotBeNull();
            schedule.MedicineId.Should().Be(createDto.MedicineId);
            schedule.FrequencyType.Should().Be(createDto.FrequencyType);
            schedule.ScheduleTimes.Should().HaveCount(2);

            // Verify Location header
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateSchedule_WithAuth_WithInvalidMedicineId_ReturnsBadRequest()
        {
            // Arrange
            AuthenticateClient();
            var createDto = new CreateScheduleDto
            {
                MedicineId = 999,
                FrequencyType = FrequencyType.Daily,
                StartDate = DateTime.UtcNow,
                Times = new List&lt;CreateScheduleTimeDto&gt;
                {
                    new CreateScheduleTimeDto { Time = new TimeSpan(8, 0, 0) }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/schedules", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateSchedule_WithAuth_ForExistingSchedule_ReturnsNoContent()
        {
            // Arrange
            AuthenticateClient();
            var existingSchedule = await _context.Schedules.FindAsync(1);
            var updateDto = new UpdateScheduleDto
            {
                FrequencyType = FrequencyType.Weekly,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddMonths(2),
                Times = new List&lt;CreateScheduleTimeDto&gt;
                {
                    new CreateScheduleTimeDto { Time = new TimeSpan(10, 0, 0) }
                }
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/schedules/{existingSchedule.Id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify schedule was updated
            var updatedSchedule = await _context.Schedules.FindAsync(existingSchedule.Id);
            updatedSchedule.FrequencyType.Should().Be(updateDto.FrequencyType);
            updatedSchedule.ScheduleTimes.Should().HaveCount(1);
        }

        [Fact]
        public async Task DeleteSchedule_WithAuth_ForExistingSchedule_ReturnsNoContent()
        {
            // Arrange
            AuthenticateClient();
            var existingSchedule = await _context.Schedules.FindAsync(1);

            // Act
            var response = await _client.DeleteAsync($"/api/schedules/{existingSchedule.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify schedule was deleted
            var deletedSchedule = await _context.Schedules.FindAsync(existingSchedule.Id);
            deletedSchedule.Should().BeNull();
        }

        public void Dispose()
        {
            // Cleanup
            _context.Database.EnsureDeleted();
            _scope.Dispose();
        }
    }
}
