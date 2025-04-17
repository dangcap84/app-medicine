using Xunit;
using Moq;
using MediTrack.Infrastructure.Services;
using MediTrack.Application.Interfaces;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.Schedule;
using MediTrack.Domain.Entities;
using MediTrack.Domain.Enums;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MediTrack.Tests.Unit.Services
{
    public class ScheduleServiceTests
    {
        private readonly Mock&lt;ApplicationDbContext&gt; _mockContext;
        private readonly Mock&lt;DbSet&lt;Schedule&gt;&gt; _mockScheduleSet;
        private readonly Mock&lt;DbSet&lt;ScheduleTime&gt;&gt; _mockScheduleTimeSet;
        private readonly ScheduleService _scheduleService;
        private readonly List&lt;Schedule&gt; _schedules;
        private readonly List&lt;ScheduleTime&gt; _scheduleTimes;

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
            dbSet.Setup(d =&gt; d.RemoveRange(It.IsAny&lt;IEnumerable&lt;T&gt;&gt;()))
                .Callback&lt;IEnumerable&lt;T&gt;&gt;(entities =&gt;
                {
                    foreach (var entity in entities.ToList())
                    {
                        sourceList.Remove(entity);
                    }
                });
            dbSet.Setup(m =&gt; m.AsNoTracking()).Returns(dbSet.Object);

            if (typeof(T) == typeof(Schedule))
            {
                dbSet.As&lt;IQueryable&lt;Schedule&gt;&gt;().Setup(m =&gt; m.Include(It.IsAny&lt;System.Linq.Expressions.Expression&lt;Func&lt;Schedule, object&gt;&gt;&gt;()))
                     .Returns(dbSet.Object as IQueryable&lt;Schedule&gt;);
            }

            return dbSet;
        }

        public ScheduleServiceTests()
        {
            _scheduleTimes = new List&lt;ScheduleTime&gt;
            {
                new ScheduleTime { Id = 1, ScheduleId = 1, Time = new TimeSpan(8, 0, 0) },
                new ScheduleTime { Id = 2, ScheduleId = 1, Time = new TimeSpan(20, 0, 0) },
                new ScheduleTime { Id = 3, ScheduleId = 2, Time = new TimeSpan(12, 0, 0) },
                new ScheduleTime { Id = 4, ScheduleId = 3, Time = new TimeSpan(9, 0, 0) }
            };

            _schedules = new List&lt;Schedule&gt;
            {
                new Schedule { Id = 1, UserId = 1, MedicineId = 1, FrequencyType = FrequencyType.Daily, StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(25), ScheduleTimes = _scheduleTimes.Where(st =&gt; st.ScheduleId == 1).ToList() },
                new Schedule { Id = 2, UserId = 1, MedicineId = 2, FrequencyType = FrequencyType.SpecificDays, SpecificDays = "Monday,Wednesday,Friday", StartDate = DateTime.UtcNow.AddDays(-10), EndDate = null, ScheduleTimes = _scheduleTimes.Where(st =&gt; st.ScheduleId == 2).ToList() },
                new Schedule { Id = 3, UserId = 2, MedicineId = 3, FrequencyType = FrequencyType.Daily, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = null, ScheduleTimes = _scheduleTimes.Where(st =&gt; st.ScheduleId == 3).ToList() }
            };

            _mockScheduleSet = CreateMockDbSet(_schedules);
            _mockScheduleTimeSet = CreateMockDbSet(_scheduleTimes);

            var options = new DbContextOptionsBuilder&lt;ApplicationDbContext&gt;()
                .UseInMemoryDatabase(databaseName: $"TestDb_Schedule_{System.Guid.NewGuid()}")
                .Options;
            _mockContext = new Mock&lt;ApplicationDbContext&gt;(options);
            _mockContext.Setup(c =&gt; c.Schedules).Returns(_mockScheduleSet.Object);
            _mockContext.Setup(c =&gt; c.ScheduleTimes).Returns(_mockScheduleTimeSet.Object);
            _mockContext.Setup(c =&gt; c.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;())).ReturnsAsync(1);

            _scheduleService = new ScheduleService(_mockContext.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetScheduleByIdAsync_ShouldThrowArgumentException_WhenScheduleIdIsInvalid(int invalidScheduleId)
        {
            // Arrange
            var userId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.GetScheduleByIdAsync(invalidScheduleId, userId));
            exception.ParamName.Should().Be("scheduleId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetScheduleByIdAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(int invalidUserId)
        {
            // Arrange
            var scheduleId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.GetScheduleByIdAsync(scheduleId, invalidUserId));
            exception.ParamName.Should().Be("userId");
        }

        [Fact]
        public async Task CreateScheduleAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            // Arrange
            var userId = 1;
            CreateScheduleDto createDto = null;

            // Act & Assert
            await Assert.ThrowsAsync&lt;ArgumentNullException&gt;(() =&gt; 
                _scheduleService.CreateScheduleAsync(createDto, userId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CreateScheduleAsync_ShouldThrowArgumentException_WhenMedicineIdIsInvalid(int invalidMedicineId)
        {
            // Arrange
            var userId = 1;
            var createDto = new CreateScheduleDto
            {
                MedicineId = invalidMedicineId,
                FrequencyType = FrequencyType.Daily,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Times = new List&lt;CreateScheduleTimeDto&gt;
                {
                    new CreateScheduleTimeDto { Time = new TimeSpan(9, 0, 0) }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.CreateScheduleAsync(createDto, userId));
            exception.ParamName.Should().Be("MedicineId");
        }

        [Fact]
        public async Task CreateScheduleAsync_ShouldThrowArgumentException_WhenTimesIsEmpty()
        {
            // Arrange
            var userId = 1;
            var createDto = new CreateScheduleDto
            {
                MedicineId = 1,
                FrequencyType = FrequencyType.Daily,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Times = new List&lt;CreateScheduleTimeDto&gt;()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.CreateScheduleAsync(createDto, userId));
            exception.Message.Should().Contain("At least one schedule time is required");
        }

        [Fact]
        public async Task CreateScheduleAsync_ShouldThrowArgumentException_WhenStartDateIsGreaterThanEndDate()
        {
            // Arrange
            var userId = 1;
            var createDto = new CreateScheduleDto
            {
                MedicineId = 1,
                FrequencyType = FrequencyType.Daily,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1),
                Times = new List&lt;CreateScheduleTimeDto&gt;
                {
                    new CreateScheduleTimeDto { Time = new TimeSpan(9, 0, 0) }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.CreateScheduleAsync(createDto, userId));
            exception.Message.Should().Contain("Start date must be less than or equal to end date");
        }

        [Fact]
        public async Task UpdateScheduleAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
        {
            // Arrange
            var scheduleId = 1;
            var userId = 1;
            UpdateScheduleDto updateDto = null;

            // Act & Assert
            await Assert.ThrowsAsync&lt;ArgumentNullException&gt;(() =&gt; 
                _scheduleService.UpdateScheduleAsync(scheduleId, updateDto, userId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task UpdateScheduleAsync_ShouldThrowArgumentException_WhenScheduleIdIsInvalid(int invalidScheduleId)
        {
            // Arrange
            var userId = 1;
            var updateDto = new UpdateScheduleDto
            {
                FrequencyType = FrequencyType.Daily,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Times = new List&lt;CreateScheduleTimeDto&gt;
                {
                    new CreateScheduleTimeDto { Time = new TimeSpan(9, 0, 0) }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.UpdateScheduleAsync(invalidScheduleId, updateDto, userId));
            exception.ParamName.Should().Be("scheduleId");
        }

        [Fact]
        public async Task UpdateScheduleAsync_ShouldThrowArgumentException_WhenTimesIsEmpty()
        {
            // Arrange
            var scheduleId = 1;
            var userId = 1;
            var updateDto = new UpdateScheduleDto
            {
                FrequencyType = FrequencyType.Daily,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Times = new List&lt;CreateScheduleTimeDto&gt;()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.UpdateScheduleAsync(scheduleId, updateDto, userId));
            exception.Message.Should().Contain("At least one schedule time is required");
        }

        [Fact]
        public async Task UpdateScheduleAsync_ShouldThrowArgumentException_WhenStartDateIsGreaterThanEndDate()
        {
            // Arrange
            var scheduleId = 1;
            var userId = 1;
            var updateDto = new UpdateScheduleDto
            {
                FrequencyType = FrequencyType.Daily,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1),
                Times = new List&lt;CreateScheduleTimeDto&gt;
                {
                    new CreateScheduleTimeDto { Time = new TimeSpan(9, 0, 0) }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.UpdateScheduleAsync(scheduleId, updateDto, userId));
            exception.Message.Should().Contain("Start date must be less than or equal to end date");
        }

        [Theory]
        [InlineData(FrequencyType.SpecificDays, null)]
        [InlineData(FrequencyType.SpecificDays, "")]
        [InlineData(FrequencyType.SpecificDays, " ")]
        public async Task UpdateScheduleAsync_ShouldThrowArgumentException_WhenSpecificDaysIsInvalidForSpecificDaysFrequency(
            FrequencyType frequencyType, string specificDays)
        {
            // Arrange
            var scheduleId = 1;
            var userId = 1;
            var updateDto = new UpdateScheduleDto
            {
                FrequencyType = frequencyType,
                SpecificDays = specificDays,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Times = new List&lt;CreateScheduleTimeDto&gt;
                {
                    new CreateScheduleTimeDto { Time = new TimeSpan(9, 0, 0) }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; 
                _scheduleService.UpdateScheduleAsync(scheduleId, updateDto, userId));
            exception.Message.Should().Contain("Specific days must be provided for SpecificDays frequency type");
        }

        // Include existing test methods here...
    }
}
