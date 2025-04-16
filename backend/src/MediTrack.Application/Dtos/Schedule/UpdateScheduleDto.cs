using MediTrack.Domain.Entities; // For FrequencyType enum
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Application.Dtos.Schedule;

// Note: Updating ScheduleTimes might be complex.
// A simpler approach for updates could be to replace all existing times with the new list provided.
// Or, handle additions/deletions/updates of individual times separately if needed.
// This DTO assumes replacement of the entire ScheduleTimes list for simplicity.

public class UpdateScheduleDto
{
    // MedicineId is generally not updated for an existing schedule.
    // If needed, delete and create a new schedule.

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public FrequencyType FrequencyType { get; set; }

    // Required if FrequencyType is Weekly
    [RequiredIf(nameof(FrequencyType), FrequencyType.Weekly, "DaysOfWeek is required for weekly frequency.")]
    [RegularExpression(@"^((Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday),)*(Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday)$", ErrorMessage = "DaysOfWeek must be a comma-separated list of valid days (e.g., Monday,Wednesday,Friday).")]
    public string? DaysOfWeek { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one schedule time is required.")]
    public List<CreateScheduleTimeDto> ScheduleTimes { get; set; } = new(); // Use Create DTO for times as we replace them
}
