using Xunit;
using Moq;
using MediTrack.Infrastructure.Services;
using MediTrack.Application.Interfaces;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.Notification;
using MediTrack.Domain.Entities;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;

namespace MediTrack.Tests.Unit.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock&lt;ApplicationDbContext&gt; _mockContext;
        private readonly Mock&lt;DbSet&lt;Notification&gt;&gt; _mockNotificationSet;
        private readonly Mock&lt;ILogger&lt;NotificationService&gt;&gt; _mockLogger;
        private readonly NotificationService _notificationService;
        private readonly List&lt;Notification&gt; _notifications;

        private static Mock&lt;DbSet&lt;T&gt;&gt; CreateMockDbSet&lt;T&gt;(List&lt;T&gt; sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock&lt;DbSet&lt;T&gt;&gt;();
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.Provider).Returns(queryable.Provider);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.Expression).Returns(queryable.Expression);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.ElementType).Returns(queryable.ElementType);
            dbSet.As&lt;IQueryable&lt;T&gt;&gt;().Setup(m =&gt; m.GetEnumerator()).Returns(() =&gt; queryable.GetEnumerator());
            dbSet.Setup(d =&gt; d.FindAsync(It.IsAny&lt;object[]&gt;()))
                 .ReturnsAsync((object[] ids) =&gt; sourceList.FirstOrDefault(d =&gt; (int)typeof(T).GetProperty("Id").GetValue(d) == (int)ids[0]));
            dbSet.Setup(m =&gt; m.AsNoTracking()).Returns(dbSet.Object);
            return dbSet;
        }

        public NotificationServiceTests()
        {
            _notifications = new List&lt;Notification&gt;
            {
                new Notification { Id = 1, UserId = 1, Message = "Uong thuoc A", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-1), DueDate = DateTime.UtcNow.AddMinutes(5) },
                new Notification { Id = 2, UserId = 1, Message = "Uong thuoc B", IsRead = true, CreatedAt = DateTime.UtcNow.AddHours(-2), DueDate = DateTime.UtcNow.AddHours(-1), UpdatedAt = DateTime.UtcNow.AddHours(-1.5) },
                new Notification { Id = 3, UserId = 1, Message = "Uong thuoc C", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-30), DueDate = DateTime.UtcNow.AddMinutes(30) },
                new Notification { Id = 4, UserId = 2, Message = "Uong thuoc D", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-1), DueDate = DateTime.UtcNow.AddMinutes(15) }
            };
            _mockNotificationSet = CreateMockDbSet(_notifications);

            var options = new DbContextOptionsBuilder&lt;ApplicationDbContext&gt;()
                .UseInMemoryDatabase(databaseName: $"TestDb_Notification_{System.Guid.NewGuid()}")
                .Options;
            _mockContext = new Mock&lt;ApplicationDbContext&gt;(options);
            _mockContext.Setup(c =&gt; c.Notifications).Returns(_mockNotificationSet.Object);
            _mockContext.Setup(c =&gt; c.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;())).ReturnsAsync(1);

            _mockLogger = new Mock&lt;ILogger&lt;NotificationService&gt;&gt;();
            _notificationService = new NotificationService(_mockContext.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetNotificationsAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(int invalidUserId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _notificationService.GetNotificationsAsync(invalidUserId, false));
            exception.ParamName.Should().Be("userId");
        }

        [Fact]
        public async Task GetNotificationsAsync_ShouldReturnAllNotificationsForUser_WhenOnlyUnreadIsFalse()
        {
            var userId = 1;

            var result = await _notificationService.GetNotificationsAsync(userId, false);

            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(n =&gt; n.UserId == userId);
        }

        [Fact]
        public async Task GetNotificationsAsync_ShouldReturnOnlyUnreadNotificationsForUser_WhenOnlyUnreadIsTrue()
        {
            var userId = 1;

            var result = await _notificationService.GetNotificationsAsync(userId, true);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(n =&gt; n.UserId == userId && !n.IsRead);
        }

        [Fact]
        public async Task GetNotificationsAsync_ShouldReturnEmptyList_WhenUserHasNoNotifications()
        {
            var userId = 3;

            var result = await _notificationService.GetNotificationsAsync(userId, false);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task MarkNotificationAsReadAsync_ShouldThrowArgumentException_WhenNotificationIdIsInvalid(int invalidNotificationId)
        {
            var userId = 1;
            var updateDto = new UpdateNotificationDto { IsRead = true };

            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _notificationService.MarkNotificationAsReadAsync(invalidNotificationId, updateDto, userId));
            exception.ParamName.Should().Be("notificationId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task MarkNotificationAsReadAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(int invalidUserId)
        {
            var notificationId = 1;
            var updateDto = new UpdateNotificationDto { IsRead = true };

            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _notificationService.MarkNotificationAsReadAsync(notificationId, updateDto, invalidUserId));
            exception.ParamName.Should().Be("userId");
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_ShouldThrowArgumentNullException_WhenUpdateDtoIsNull()
        {
            var notificationId = 1;
            var userId = 1;
            UpdateNotificationDto updateDto = null;

            await Assert.ThrowsAsync&lt;ArgumentNullException&gt;(() =&gt; 
                _notificationService.MarkNotificationAsReadAsync(notificationId, updateDto, userId));
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_ShouldMarkAsRead_WhenFoundAndBelongsToUser()
        {
            var notificationId = 1;
            var userId = 1;
            var updateDto = new UpdateNotificationDto { IsRead = true };

            var existingNotification = _notifications.First(n =&gt; n.Id == notificationId);
            _mockNotificationSet.Setup(d =&gt; d.FindAsync(It.Is&lt;object[]&gt;(ids =&gt; (int)ids[0] == notificationId)))
                           .ReturnsAsync(existingNotification);

            var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, updateDto, userId);

            result.Should().BeTrue();
            existingNotification.IsRead.Should().BeTrue();
            existingNotification.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Once);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_ShouldReturnFalse_WhenNotFound()
        {
            var notificationId = 99;
            var userId = 1;
            var updateDto = new UpdateNotificationDto { IsRead = true };

            _mockNotificationSet.Setup(d =&gt; d.FindAsync(It.Is&lt;object[]&gt;(ids =&gt; (int)ids[0] == notificationId)))
                           .ReturnsAsync((Notification)null);

            var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, updateDto, userId);

            result.Should().BeFalse();
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Never);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_ShouldReturnFalse_WhenNotBelongsToUser()
        {
            var notificationId = 4;
            var userId = 1;
            var updateDto = new UpdateNotificationDto { IsRead = true };

            var existingNotification = _notifications.First(n =&gt; n.Id == notificationId);
            _mockNotificationSet.Setup(d =&gt; d.FindAsync(It.Is&lt;object[]&gt;(ids =&gt; (int)ids[0] == notificationId)))
                           .ReturnsAsync(existingNotification);

            var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, updateDto, userId);

            result.Should().BeFalse();
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Never);
        }
    }
}
