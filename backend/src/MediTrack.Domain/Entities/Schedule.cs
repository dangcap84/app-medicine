using System;
using System.Collections.Generic;

namespace MediTrack.Domain.Entities;

public enum FrequencyType
{
    Daily,
    Weekly,
    SpecificDays // Added for schedules on specific days of the week
    // Add other types if needed, e.g., Monthly, SpecificInterval
}

public class Schedule
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid MedicineId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public FrequencyType FrequencyType { get; set; } // Use the enum
    public string? DaysOfWeek { get; set; } // e.g., "Mon,Wed,Fri" for SpecificDays
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
    public ICollection<ScheduleTime> ScheduleTimes { get; set; } = new List<ScheduleTime>();
}
