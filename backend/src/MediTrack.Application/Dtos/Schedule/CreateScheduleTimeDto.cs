using System;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Application.Dtos.Schedule;

public class CreateScheduleTimeDto
{
    [Required]
    public TimeOnly TimeOfDay { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
