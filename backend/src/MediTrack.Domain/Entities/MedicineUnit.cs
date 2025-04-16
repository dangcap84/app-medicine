using System;
using System.Collections.Generic;

namespace MediTrack.Domain.Entities;

public class MedicineUnit
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!; // e.g., "viên", "ml", "ống"
    public string? Description { get; set; }

    // Navigation property
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}
