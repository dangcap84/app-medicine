using Xunit;
using Moq;
using MediTrack.Infrastructure.Services;
using MediTrack.Application.Interfaces;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.UserProfile;
using MediTrack.Domain.Entities;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MediTrack.Tests.Unit.Services
{
    public class UserProfileServiceTests
    {
        private readonly Mock&lt;ApplicationDbContext&gt; _mockContext;
        private readonly Mock&lt;DbSet&lt;UserProfile&gt;&gt; _mockUserProfileSet;
        private readonly UserProfileService _userProfileService;
        private readonly List&lt;UserProfile&gt; _userProfiles;

        private static Mock&lt;DbSet&lt;T&gt;&gt; CreateMockDbSet&lt;T&gt;(List&lt;T&gt; sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock&lt;DbSet&lt;T&gt;&gt;();
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.Provider).Returns(queryable.Provider);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.Expression).Returns(queryable.Expression);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.ElementType).Returns(queryable.ElementType);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.GetEnumerator()).Returns(() =&gt; queryable.GetEnumerator());
            dbSet.Setup(d =&gt; d.FindAsync(It.IsAny&lt;object[]&gt;()))
               .ReturnsAsync((object[] ids) =&gt; sourceList.FirstOrDefault(d =&gt; (int)typeof(T).GetProperty("UserId").GetValue(d) == (int)ids[0]));
            dbSet.Setup(m =&gt; m.AsNoTracking()).Returns(dbSet.Object);
            return dbSet;
        }

        public UserProfileServiceTests()
        {
            _userProfiles = new List&lt;UserProfile&gt;
            {
                new UserProfile { UserId = 1, FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1990, 1, 1), Gender = "Male" },
                new UserProfile { UserId = 2, FirstName = "Jane", LastName = "Smith", DateOfBirth = new DateTime(1985, 5, 15), Gender = "Female" }
            };
            _mockUserProfileSet = CreateMockDbSet(_userProfiles);

            var options = new DbContextOptionsBuilder&lt;ApplicationDbContext&gt;()
                .UseInMemoryDatabase(databaseName: $"TestDb_UserProfile_{System.Guid.NewGuid()}")
                .Options;
            _mockContext = new Mock&lt;ApplicationDbContext&gt;(options);
            _mockContext.Setup(c =&gt; c.UserProfiles).Returns(_mockUserProfileSet.Object);
            _mockContext.Setup(c =&gt; c.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;())).ReturnsAsync(1);

            _userProfileService = new UserProfileService(_mockContext.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetUserProfileAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(int invalidUserId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _userProfileService.GetUserProfileAsync(invalidUserId));
            exception.ParamName.Should().Be("userId");
        }

        [Fact]
        public async Task GetUserProfileAsync_ShouldReturnProfile_WhenExists()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _userProfileService.GetUserProfileAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.FirstName.Should().Be("John");
        }

        [Fact]
        public async Task GetUserProfileAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var userId = 99;

            // Act
            var result = await _userProfileService.GetUserProfileAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            // Arrange
            var userId = 1;
            UpdateUserProfileDto updateDto = null;

            // Act & Assert
            await Assert.ThrowsAsync&lt;ArgumentNullException&gt;(() =&gt; 
                _userProfileService.UpdateUserProfileAsync(userId, updateDto));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateUserProfileAsync_ShouldThrowArgumentException_WhenFirstNameIsInvalid(string invalidFirstName)
        {
            // Arrange
            var userId = 1;
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = invalidFirstName,
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _userProfileService.UpdateUserProfileAsync(userId, updateDto));
            exception.Message.Should().Contain("FirstName is required");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateUserProfileAsync_ShouldThrowArgumentException_WhenLastNameIsInvalid(string invalidLastName)
        {
            // Arrange
            var userId = 1;
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = invalidLastName,
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _userProfileService.UpdateUserProfileAsync(userId, updateDto));
            exception.Message.Should().Contain("LastName is required");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("InvalidGender")]
        public async Task UpdateUserProfileAsync_ShouldThrowArgumentException_WhenGenderIsInvalid(string invalidGender)
        {
            // Arrange
            var userId = 1;
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = invalidGender
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _userProfileService.UpdateUserProfileAsync(userId, updateDto));
            exception.Message.Should().Contain("Gender must be either 'Male' or 'Female'");
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldThrowArgumentException_WhenDateOfBirthIsInFuture()
        {
            // Arrange
            var userId = 1;
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.UtcNow.AddDays(1),
                Gender = "Male"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _userProfileService.UpdateUserProfileAsync(userId, updateDto));
            exception.Message.Should().Contain("Date of birth cannot be in the future");
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldUpdateProfile_WhenExists()
        {
            // Arrange
            var userId = 1;
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "John Updated",
                LastName = "Doe Updated",
                DateOfBirth = new DateTime(1991, 2, 2),
                Gender = "Male"
            };

            var existingProfile = _userProfiles.First(p =&gt; p.UserId == userId);
            _mockUserProfileSet.Setup(d =&gt; d.FindAsync(It.Is&lt;object[]&gt;(ids =&gt; (int)ids[0] == userId)))
                           .ReturnsAsync(existingProfile);

            // Act
            var result = await _userProfileService.UpdateUserProfileAsync(userId, updateDto);

            // Assert
            result.Should().BeTrue();
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Once);

            existingProfile.FirstName.Should().Be(updateDto.FirstName);
            existingProfile.LastName.Should().Be(updateDto.LastName);
            existingProfile.DateOfBirth.Should().Be(updateDto.DateOfBirth);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldReturnFalse_WhenNotFound()
        {
            // Arrange
            var userId = 99;
            var updateDto = new UpdateUserProfileDto
            {
                FirstName = "NonExistent",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male"
            };

            _mockUserProfileSet.Setup(d =&gt; d.FindAsync(It.Is&lt;object[]&gt;(ids =&gt; (int)ids[0] == userId)))
                           .ReturnsAsync((UserProfile)null);

            // Act
            var result = await _userProfileService.UpdateUserProfileAsync(userId, updateDto);

            // Assert
            result.Should().BeFalse();
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Never);
        }
    }
}
