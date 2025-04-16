using System;

namespace MediTrack.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ScheduleTimeId { get; set; }
    public DateTime ScheduledTime { get; set; } // Thời gian dự kiến gửi thông báo
    public DateTime? SentTime { get; set; } // Thời gian thực tế gửi
    public bool IsRead { get; set; } = false;
    public string Message { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ScheduleTime ScheduleTime { get; set; } = null!;
}
