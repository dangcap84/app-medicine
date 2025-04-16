using System;
using System.Collections.Generic;

namespace MediTrack.Domain.Entities;

public class ScheduleTime
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public TimeOnly TimeOfDay { get; set; } // Using TimeOnly for time without date
    public int Quantity { get; set; } // Số lượng uống mỗi lần

    // Navigation properties
    public Schedule Schedule { get; set; } = null!;
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
