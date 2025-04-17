using Xunit;
using Moq;
using MediTrack.Infrastructure.Services;
using MediTrack.Application.Interfaces;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.Medicine;
using MediTrack.Domain.Entities;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MediTrack.Tests.Unit.Services
{
    public class MedicineServiceTests
    {
        private readonly Mock&lt;ApplicationDbContext&gt; _mockContext;
        private readonly Mock&lt;DbSet&lt;Medicine&gt;&gt; _mockMedicineSet;
        private readonly MedicineService _medicineService;
        private readonly List&lt;Medicine&gt; _medicines;

        private static Mock&lt;DbSet&lt;T&gt;&gt; CreateMockDbSet&lt;T&gt;(List&lt;T&gt; sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock&lt;DbSet&lt;T&gt;&gt;();
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.Provider).Returns(queryable.Provider);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.Expression).Returns(queryable.Expression);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.ElementType).Returns(queryable.ElementType);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.GetEnumerator()).Returns(() =&gt; queryable.GetEnumerator());
            dbSet.Setup(d =&gt; d.AddAsync(It.IsAny&lt;T&gt;(), It.IsAny&lt;CancellationToken&gt;()))
                 .Callback&lt;T, CancellationToken&gt;((s, ct) =&gt; sourceList.Add(s));
            dbSet.Setup(d =&gt; d.FindAsync(It.IsAny&lt;object[]&gt;()))
               .ReturnsAsync((object[] ids) =&gt; sourceList.FirstOrDefault(d =&gt; (int)typeof(T).GetProperty("Id").GetValue(d) == (int)ids[0]));
            dbSet.Setup(d =&gt; d.Remove(It.IsAny&lt;T&gt;()))
               .Callback&lt;T&gt;(s =&gt; sourceList.Remove(s));
            dbSet.Setup(m =&gt; m.AsNoTracking()).Returns(dbSet.Object);
            return dbSet;
        }

        public MedicineServiceTests()
        {
            _medicines = new List&lt;Medicine&gt;
            {
                new Medicine { Id = 1, UserId = 1, Name = "Medicine A", Dosage = "10mg", Frequency = "Daily", StartedDate = DateTime.UtcNow.AddDays(-10) },
                new Medicine { Id = 2, UserId = 1, Name = "Medicine B", Dosage = "5ml", Frequency = "Twice Daily", StartedDate = DateTime.UtcNow.AddDays(-5) },
                new Medicine { Id = 3, UserId = 2, Name = "Medicine C", Dosage = "1 tablet", Frequency = "Weekly", StartedDate = DateTime.UtcNow.AddDays(-20) }
            };
            _mockMedicineSet = CreateMockDbSet(_medicines);

            var options = new DbContextOptionsBuilder&lt;ApplicationDbContext&gt;()
                .UseInMemoryDatabase(databaseName: $"TestDb_Medicine_{System.Guid.NewGuid()}")
                .Options;
            _mockContext = new Mock&lt;ApplicationDbContext&gt;(options);
            _mockContext.Setup(c =&gt; c.Medicines).Returns(_mockMedicineSet.Object);
            _mockContext.Setup(c =&gt; c.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;())).ReturnsAsync(1);

            _medicineService = new MedicineService(_mockContext.Object);
        }

        [Fact]
        public async Task GetMedicineByIdAsync_ShouldReturnMedicine_WhenMedicineExists()
        {
            var medicineId = 1;
            var userId = 1;

            var result = await _medicineService.GetMedicineByIdAsync(medicineId, userId);

            result.Should().NotBeNull();
            result.Id.Should().Be(medicineId);
            result.Name.Should().Be("Medicine A");
        }

        [Fact]
        public async Task GetMedicineByIdAsync_ShouldReturnNull_WhenMedicineDoesNotExist()
        {
            var medicineId = 99;
            var userId = 1;

            var result = await _medicineService.GetMedicineByIdAsync(medicineId, userId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetMedicineByIdAsync_ShouldReturnNull_WhenMedicineBelongsToDifferentUser()
        {
            var medicineId = 3;
            var userId = 1;

            var result = await _medicineService.GetMedicineByIdAsync(medicineId, userId);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetMedicineByIdAsync_ShouldThrowArgumentException_WhenMedicineIdIsInvalid(int invalidMedicineId)
        {
            // Arrange
            var userId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _medicineService.GetMedicineByIdAsync(invalidMedicineId, userId));
            exception.ParamName.Should().Be("medicineId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetMedicineByIdAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(int invalidUserId)
        {
            // Arrange
            var medicineId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _medicineService.GetMedicineByIdAsync(medicineId, invalidUserId));
            exception.ParamName.Should().Be("userId");
        }

        [Fact]
        public async Task GetMedicinesByUserIdAsync_ShouldReturnUserMedicines()
        {
            var userId = 1;

            var results = await _medicineService.GetMedicinesByUserIdAsync(userId);

            results.Should().NotBeNull();
            results.Should().HaveCount(2);
            results.Should().OnlyContain(m =&gt; m.UserId == userId);
            results.Select(m =&gt; m.Name).Should().Contain(new[] { "Medicine A", "Medicine B" });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetMedicinesByUserIdAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(int invalidUserId)
        {
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _medicineService.GetMedicinesByUserIdAsync(invalidUserId));
            exception.ParamName.Should().Be("userId");
        }

        [Fact]
        public async Task CreateMedicineAsync_ShouldAddMedicine()
        {
            var userId = 1;
            var createDto = new CreateMedicineDto
            {
                Name = "New Medicine D",
                Dosage = "20mg",
                Frequency = "Once Daily",
                StartedDate = DateTime.UtcNow
            };

            var result = await _medicineService.CreateMedicineAsync(createDto, userId);

            result.Should().NotBeNull();
            result.Name.Should().Be(createDto.Name);
            result.UserId.Should().Be(userId);
            _mockMedicineSet.Verify(m =&gt; m.AddAsync(It.Is&lt;Medicine&gt;(med =&gt; med.Name == createDto.Name && med.UserId == userId), It.IsAny&lt;CancellationToken&gt;()), Times.Once);
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Once);
        }

        [Fact]
        public async Task CreateMedicineAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            var userId = 1;
            CreateMedicineDto createDto = null;

            await Assert.ThrowsAsync&lt;ArgumentNullException&gt;(() =&gt; 
                _medicineService.CreateMedicineAsync(createDto, userId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task CreateMedicineAsync_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
        {
            var userId = 1;
            var createDto = new CreateMedicineDto
            {
                Name = invalidName,
                Dosage = "20mg",
                Frequency = "Once Daily",
                StartedDate = DateTime.UtcNow
            };

            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _medicineService.CreateMedicineAsync(createDto, userId));
            exception.ParamName.Should().Be("Name");
        }

        [Fact]
        public async Task UpdateMedicineAsync_ShouldUpdateMedicine_WhenFoundAndBelongsToUser()
        {
            var medicineId = 1;
            var userId = 1;
            var updateDto = new UpdateMedicineDto
            {
                Name = "Medicine A Updated",
                Dosage = "15mg",
                Frequency = "Daily Updated",
                StartedDate = DateTime.UtcNow.AddDays(-11)
            };

            var result = await _medicineService.UpdateMedicineAsync(medicineId, updateDto, userId);

            result.Should().BeTrue();
            var updatedEntity = await _mockContext.Object.Medicines.FindAsync(medicineId);
            updatedEntity.Should().NotBeNull();
            updatedEntity.Name.Should().Be(updateDto.Name);
            updatedEntity.Dosage.Should().Be(updateDto.Dosage);
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Once);
        }

        [Fact]
        public async Task UpdateMedicineAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            var medicineId = 1;
            var userId = 1;
            UpdateMedicineDto updateDto = null;

            await Assert.ThrowsAsync&lt;ArgumentNullException&gt;(() =&gt; 
                _medicineService.UpdateMedicineAsync(medicineId, updateDto, userId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateMedicineAsync_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
        {
            var medicineId = 1;
            var userId = 1;
            var updateDto = new UpdateMedicineDto
            {
                Name = invalidName,
                Dosage = "15mg",
                Frequency = "Daily Updated",
                StartedDate = DateTime.UtcNow.AddDays(-11)
            };

            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _medicineService.UpdateMedicineAsync(medicineId, updateDto, userId));
            exception.ParamName.Should().Be("Name");
        }

        [Fact]
        public async Task UpdateMedicineAsync_ShouldReturnFalse_WhenNotFound()
        {
            var medicineId = 99;
            var userId = 1;
            var updateDto = new UpdateMedicineDto { Name = "NonExistent" };

            var result = await _medicineService.UpdateMedicineAsync(medicineId, updateDto, userId);

            result.Should().BeFalse();
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Never);
        }

        [Fact]
        public async Task UpdateMedicineAsync_ShouldReturnFalse_WhenNotBelongsToUser()
        {
            var medicineId = 3;
            var userId = 1;
            var updateDto = new UpdateMedicineDto { Name = "Trying To Update" };

            var result = await _medicineService.UpdateMedicineAsync(medicineId, updateDto, userId);

            result.Should().BeFalse();
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Never);
        }

        [Fact]
        public async Task DeleteMedicineAsync_ShouldRemoveMedicine_WhenFoundAndBelongsToUser()
        {
            var medicineId = 1;
            var userId = 1;

            var result = await _medicineService.DeleteMedicineAsync(medicineId, userId);

            result.Should().BeTrue();
            _mockMedicineSet.Verify(m =&gt; m.Remove(It.Is&lt;Medicine&gt;(med =&gt; med.Id == medicineId)), Times.Once);
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteMedicineAsync_ShouldThrowArgumentException_WhenMedicineIdIsInvalid(int invalidMedicineId)
        {
            var userId = 1;

            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _medicineService.DeleteMedicineAsync(invalidMedicineId, userId));
            exception.ParamName.Should().Be("medicineId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteMedicineAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(int invalidUserId)
        {
            var medicineId = 1;

            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _medicineService.DeleteMedicineAsync(medicineId, invalidUserId));
            exception.ParamName.Should().Be("userId");
        }

        [Fact]
        public async Task DeleteMedicineAsync_ShouldReturnFalse_WhenNotFound()
        {
            var medicineId = 99;
            var userId = 1;

            var result = await _medicineService.DeleteMedicineAsync(medicineId, userId);

            result.Should().BeFalse();
            _mockMedicineSet.Verify(m =&gt; m.Remove(It.IsAny&lt;Medicine&gt;()), Times.Never);
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Never);
        }

        [Fact]
        public async Task DeleteMedicineAsync_ShouldReturnFalse_WhenNotBelongsToUser()
        {
            var medicineId = 3;
            var userId = 1;

            var result = await _medicineService.DeleteMedicineAsync(medicineId, userId);

            result.Should().BeFalse();
            _mockMedicineSet.Verify(m =&gt; m.Remove(It.IsAny&lt;Medicine&gt;()), Times.Never);
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Never);
        }
    }
}
