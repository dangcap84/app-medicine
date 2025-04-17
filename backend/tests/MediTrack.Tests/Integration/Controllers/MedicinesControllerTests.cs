using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediTrack.Api;
using MediTrack.Application.Dtos.Auth;
using MediTrack.Application.Dtos.Medicine;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

namespace MediTrack.Tests.Integration.Controllers
{
    public class MedicinesControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;, IDisposable
    {
        private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;
        private string _authToken;
        private int _userId;

        public MedicinesControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
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

            // Create test medicines
            var medicines = new List&lt;Medicine&gt;
            {
                new Medicine 
                { 
                    UserId = user.Id,
                    Name = "Medicine A",
                    Dosage = "10mg",
                    Notes = "Take with food",
                    StartedDate = DateTime.UtcNow.AddDays(-5)
                },
                new Medicine 
                { 
                    UserId = user.Id,
                    Name = "Medicine B",
                    Dosage = "5ml",
                    Notes = "Take before bed",
                    StartedDate = DateTime.UtcNow.AddDays(-3)
                }
            };
            _context.Medicines.AddRange(medicines);
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
        public async Task GetMedicines_WithoutAuth_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/medicines");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMedicines_WithAuth_ReturnsAllMedicinesForUser()
        {
            // Arrange
            AuthenticateClient();

            // Act
            var response = await _client.GetAsync("/api/medicines");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var medicines = await response.Content.ReadFromJsonAsync&lt;List&lt;MedicineDto&gt;&gt;();
            medicines.Should().HaveCount(2);
            medicines.Should().OnlyContain(m =&gt; m.UserId == _userId);
        }

        [Fact]
        public async Task GetMedicine_WithAuth_ForExistingMedicine_ReturnsMedicine()
        {
            // Arrange
            AuthenticateClient();
            var existingMedicine = await _context.Medicines.FindAsync(1);

            // Act
            var response = await _client.GetAsync($"/api/medicines/{existingMedicine.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var medicine = await response.Content.ReadFromJsonAsync&lt;MedicineDto&gt;();
            medicine.Should().NotBeNull();
            medicine.Name.Should().Be(existingMedicine.Name);
        }

        [Fact]
        public async Task GetMedicine_WithAuth_ForNonExistentMedicine_ReturnsNotFound()
        {
            // Arrange
            AuthenticateClient();

            // Act
            var response = await _client.GetAsync("/api/medicines/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateMedicine_WithAuth_WithValidData_ReturnsCreatedMedicine()
        {
            // Arrange
            AuthenticateClient();
            var createDto = new CreateMedicineDto
            {
                Name = "New Medicine",
                Dosage = "15mg",
                Notes = "Test notes",
                StartedDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/medicines", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var medicine = await response.Content.ReadFromJsonAsync&lt;MedicineDto&gt;();
            medicine.Should().NotBeNull();
            medicine.Name.Should().Be(createDto.Name);
            medicine.UserId.Should().Be(_userId);

            // Verify Location header
            response.Headers.Location.Should().NotBeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CreateMedicine_WithAuth_WithInvalidName_ReturnsBadRequest(string invalidName)
        {
            // Arrange
            AuthenticateClient();
            var createDto = new CreateMedicineDto
            {
                Name = invalidName,
                Dosage = "15mg",
                Notes = "Test notes",
                StartedDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/medicines", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateMedicine_WithAuth_ForExistingMedicine_ReturnsNoContent()
        {
            // Arrange
            AuthenticateClient();
            var existingMedicine = await _context.Medicines.FindAsync(1);
            var updateDto = new UpdateMedicineDto
            {
                Name = "Updated Medicine",
                Dosage = "20mg",
                Notes = "Updated notes",
                StartedDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/medicines/{existingMedicine.Id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify medicine was updated
            var updatedMedicine = await _context.Medicines.FindAsync(existingMedicine.Id);
            updatedMedicine.Name.Should().Be(updateDto.Name);
            updatedMedicine.Dosage.Should().Be(updateDto.Dosage);
        }

        [Fact]
        public async Task UpdateMedicine_WithAuth_ForNonExistentMedicine_ReturnsNotFound()
        {
            // Arrange
            AuthenticateClient();
            var updateDto = new UpdateMedicineDto
            {
                Name = "NonExistent Medicine",
                Dosage = "20mg",
                Notes = "Test notes",
                StartedDate = DateTime.UtcNow
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/medicines/999", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteMedicine_WithAuth_ForExistingMedicine_ReturnsNoContent()
        {
            // Arrange
            AuthenticateClient();
            var existingMedicine = await _context.Medicines.FindAsync(1);

            // Act
            var response = await _client.DeleteAsync($"/api/medicines/{existingMedicine.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify medicine was deleted
            var deletedMedicine = await _context.Medicines.FindAsync(existingMedicine.Id);
            deletedMedicine.Should().BeNull();
        }

        [Fact]
        public async Task DeleteMedicine_WithAuth_ForNonExistentMedicine_ReturnsNotFound()
        {
            // Arrange
            AuthenticateClient();

            // Act
            var response = await _client.DeleteAsync("/api/medicines/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        public void Dispose()
        {
            // Cleanup
            _context.Database.EnsureDeleted();
            _scope.Dispose();
        }
    }
}
