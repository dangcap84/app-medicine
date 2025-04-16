using System;
using System.ComponentModel.DataAnnotations;

namespace MediTrack.Application.Dtos.Medicine;

public class CreateMedicineDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Dosage { get; set; } = null!;

    [Required]
    public Guid MedicineUnitId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // UserId will be obtained from the authenticated user context (JWT token)
}
