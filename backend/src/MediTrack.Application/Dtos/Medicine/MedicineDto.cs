using System;

namespace MediTrack.Application.Dtos.Medicine;

public class MedicineDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Dosage { get; set; } = null!;
    public Guid MedicineUnitId { get; set; }
    public string MedicineUnitName { get; set; } = null!; // Include unit name for convenience
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
