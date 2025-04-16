using MediTrack.Domain.Entities; // For FrequencyType enum
using System;
using System.Collections.Generic;

namespace MediTrack.Application.Dtos.Schedule;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = null!; // Include medicine name
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public FrequencyType FrequencyType { get; set; }
    public string? DaysOfWeek { get; set; } // Comma-separated days or null
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ScheduleTimeDto> ScheduleTimes { get; set; } = new();
}
