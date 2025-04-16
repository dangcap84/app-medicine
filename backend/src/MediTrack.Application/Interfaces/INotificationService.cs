using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.Notification;

namespace MediTrack.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for services related to notification management,
    /// including generation based on schedules.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Scans active schedules and generates upcoming notification records in the database.
        /// </summary>
        /// <param name="lookAheadTime">How far into the future to generate notifications for (e.g., 24 hours).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task GenerateNotificationsAsync(TimeSpan lookAheadTime);

        /// <summary>
        /// Gets all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="includeRead">Whether to include notifications that have been read.</param>
        /// <returns>A list of notifications.</returns>
        Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId, bool includeRead = false);

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="notificationId">The notification's ID.</param>
        /// <returns>True if the notification was marked as read, false if not found or already read.</returns>
        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);

        /// <summary>
        /// Deletes a notification.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="notificationId">The notification's ID.</param>
        /// <returns>True if the notification was deleted, false if not found.</returns>
        Task<bool> DeleteNotificationAsync(Guid userId, Guid notificationId);
    }
}
