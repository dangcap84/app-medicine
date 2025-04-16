using System.Threading.Tasks;

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

        // Other notification-related methods (like getting notifications, marking as read)
        // will be added later as part of the Notification API (Step 9).
    }
}
