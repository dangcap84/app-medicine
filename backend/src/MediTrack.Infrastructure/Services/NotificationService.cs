using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediTrack.Application.Interfaces;
using MediTrack.Application.Dtos.Notification;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediTrack.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger; // Inject logger

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task GenerateNotificationsAsync(TimeSpan lookAheadTime)
        {
            _logger.LogInformation("Starting notification generation process at {UtcNow}", DateTime.UtcNow);

            var now = DateTime.UtcNow;
            var lookAheadUntil = now.Add(lookAheadTime);

            // Get active schedules with their times and medicine info
            var activeSchedules = await _context.Schedules
                .Include(s => s.ScheduleTimes)
                .Include(s => s.Medicine) // Include Medicine to get the name for the message
                .Where(s => s.StartDate <= lookAheadUntil && (s.EndDate == null || s.EndDate >= now))
                .ToListAsync();

            _logger.LogInformation("Found {Count} active schedules to process.", activeSchedules.Count);

            var notificationsToCreate = new List<Notification>();

            foreach (var schedule in activeSchedules)
            {
                foreach (var scheduleTime in schedule.ScheduleTimes)
                {
                    // Calculate the specific notification times within the look-ahead window
                    var notificationTimes = CalculateNotificationTimes(schedule, scheduleTime, now, lookAheadUntil);

                    foreach (var scheduledTime in notificationTimes)
                    {
                        // Check if a notification for this specific user, schedule time, and exact scheduled time already exists
                        bool notificationExists = await _context.Notifications
                            .AnyAsync(n => n.UserId == schedule.UserId &&
                                           n.ScheduleTimeId == scheduleTime.Id &&
                                           n.ScheduledTime == scheduledTime);

                        if (!notificationExists)
                        {
                            // Create notification message (customize as needed)
                            string message = $"Đã đến giờ uống {schedule.Medicine.Name} ({scheduleTime.Quantity} {schedule.Medicine.Dosage})"; // Assuming Dosage includes unit like 'viên'

                            notificationsToCreate.Add(new Notification
                            {
                                Id = Guid.NewGuid(),
                                UserId = schedule.UserId,
                                ScheduleTimeId = scheduleTime.Id,
                                ScheduledTime = scheduledTime,
                                Message = message,
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                                // SentTime will be updated when the notification is actually sent (e.g., via FCM)
                            });
                            _logger.LogDebug("Prepared notification for User {UserId}, Medicine {MedicineName} at {ScheduledTime}", schedule.UserId, schedule.Medicine.Name, scheduledTime);
                        }
                        else
                        {
                             _logger.LogDebug("Notification already exists for User {UserId}, ScheduleTime {ScheduleTimeId} at {ScheduledTime}", schedule.UserId, scheduleTime.Id, scheduledTime);
                        }
                    }
                }
            }

            if (notificationsToCreate.Any())
            {
                _context.Notifications.AddRange(notificationsToCreate);
                try
                {
                    var savedCount = await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully generated and saved {Count} new notifications.", savedCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving generated notifications to the database.");
                    // Consider re-throwing or handling the exception appropriately
                }
            }
            else
            {
                _logger.LogInformation("No new notifications needed to be generated in this run.");
            }
        }

        // Helper method to calculate specific notification datetimes based on schedule frequency
        private List<DateTime> CalculateNotificationTimes(Schedule schedule, ScheduleTime scheduleTime, DateTime startTime, DateTime endTime)
        {
            var notificationTimes = new List<DateTime>();
            var timeOfDay = scheduleTime.TimeOfDay; // TimeOnly

            // Iterate through each day in the look-ahead window
            for (var date = startTime.Date; date <= endTime.Date; date = date.AddDays(1))
            {
                // Combine date and time, ensuring it's within the look-ahead window
                var potentialNotificationTime = date.Add(timeOfDay.ToTimeSpan()); // Convert TimeOnly to TimeSpan
                 potentialNotificationTime = DateTime.SpecifyKind(potentialNotificationTime, DateTimeKind.Utc); // Ensure it's UTC


                // Check if the potential time is within the schedule's active range and the look-ahead window
                if (potentialNotificationTime >= startTime && potentialNotificationTime < endTime &&
                    potentialNotificationTime >= schedule.StartDate &&
                    (schedule.EndDate == null || potentialNotificationTime <= schedule.EndDate))
                {
                    // Check frequency
                    bool shouldNotify = false;
                    switch (schedule.FrequencyType)
                    {
                        case FrequencyType.Daily:
                            shouldNotify = true;
                            break;
                        case FrequencyType.SpecificDays:
                            if (!string.IsNullOrEmpty(schedule.DaysOfWeek))
                            {
                                var days = schedule.DaysOfWeek.Split(',').Select(d => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), d.Trim(), true));
                                if (days.Contains(date.DayOfWeek))
                                {
                                    shouldNotify = true;
                                }
                            }
                            break;
                        // Add cases for Weekly, Monthly if needed
                        default:
                             _logger.LogWarning("Unsupported FrequencyType {FrequencyType} encountered for Schedule {ScheduleId}", schedule.FrequencyType, schedule.Id);
                            break;
                    }

                    if (shouldNotify)
                    {
                        notificationTimes.Add(potentialNotificationTime);
                    }
                }
            }
            return notificationTimes;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId, bool includeRead = false)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            if (!includeRead)
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query
                .OrderByDescending(n => n.ScheduledTime)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    ScheduleTimeId = n.ScheduleTimeId,
                    Message = n.Message,
                    ScheduledTime = n.ScheduledTime,
                    IsRead = n.IsRead,
                    SentTime = n.SentTime,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null || notification.IsRead)
            {
                return false;
            }

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrency conflict when marking notification {NotificationId} as read", notificationId);
                return false;
            }
        }

        public async Task<bool> DeleteNotificationAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            try
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
                return false;
            }
        }
    }
}
