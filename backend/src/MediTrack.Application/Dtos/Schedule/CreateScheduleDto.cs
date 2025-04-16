using MediTrack.Domain.Entities; // For FrequencyType enum
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Application.Dtos.Schedule;

public class CreateScheduleDto
{
    [Required]
    public Guid MedicineId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public FrequencyType FrequencyType { get; set; }
 
     // Required if FrequencyType is Weekly
     [RequiredIf(nameof(FrequencyType), FrequencyType.Weekly, "DaysOfWeek is required for weekly frequency.")] // Pass error message directly
     [RegularExpression(@"^((Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday),)*(Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday)$", ErrorMessage = "DaysOfWeek must be a comma-separated list of valid days (e.g., Monday,Wednesday,Friday).")]
     public string? DaysOfWeek { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one schedule time is required.")]
    public List<CreateScheduleTimeDto> ScheduleTimes { get; set; } = new();
}

// Helper class for conditional validation (can be moved to a shared location later)
public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _propertyName;
    private readonly object _desiredValue;

    public RequiredIfAttribute(string propertyName, object desiredValue, string errorMessage) : base(errorMessage)
    {
        _propertyName = propertyName;
        _desiredValue = desiredValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        var propertyValue = type.GetProperty(_propertyName)?.GetValue(instance, null);

        if (propertyValue?.Equals(_desiredValue) == true)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}
