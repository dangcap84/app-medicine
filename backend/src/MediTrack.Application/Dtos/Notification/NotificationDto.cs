using System;

namespace MediTrack.Application.Dtos.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ScheduleTimeId { get; set; }
        public string Message { get; set; }
        public DateTime ScheduledTime { get; set; }
        public bool IsRead { get; set; }
        public DateTime? SentTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
