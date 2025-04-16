using System;

namespace MediTrack.Application.Dtos.Schedule;

public class ScheduleTimeDto
{
    public Guid Id { get; set; }
    public TimeOnly TimeOfDay { get; set; }
    public int Quantity { get; set; }
}
