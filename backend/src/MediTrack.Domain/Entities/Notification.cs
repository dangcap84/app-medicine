using System;

namespace MediTrack.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ScheduleTimeId { get; set; }
    public string Message { get; set; }
    public DateTime ScheduledTime { get; set; }
    public bool IsRead { get; set; }
    public DateTime? SentTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ScheduleTime ScheduleTime { get; set; } = null!;
}
