using System;
using System.Threading.Tasks;
using MediTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets all notifications for the current user.
        /// </summary>
        /// <param name="includeRead">Whether to include notifications that have been read (default: false).</param>
        /// <returns>A list of notifications.</returns>
        /// <response code="200">Returns the list of notifications.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetNotifications([FromQuery] bool includeRead = false)
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetNotificationsAsync(userId, includeRead);
            return Ok(notifications);
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the notification was marked as read.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the notification was not found or does not belong to the user.</response>
        [HttpPut("{id}/read")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetUserId();
            var success = await _notificationService.MarkAsReadAsync(userId, id);
            
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a notification.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">If the notification was deleted.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the notification was not found or does not belong to the user.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var userId = GetUserId();
            var success = await _notificationService.DeleteNotificationAsync(userId, id);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user identifier.");
            }
            return userId;
        }
    }
}
