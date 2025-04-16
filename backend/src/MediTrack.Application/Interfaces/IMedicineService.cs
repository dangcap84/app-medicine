using MediTrack.Application.Dtos.Medicine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediTrack.Application.Interfaces;

public interface IMedicineService
{
    Task<MedicineDto?> GetMedicineByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<MedicineDto>> GetMedicinesByUserIdAsync(Guid userId);
    Task<MedicineDto?> CreateMedicineAsync(CreateMedicineDto medicineDto, Guid userId);
    Task<bool> UpdateMedicineAsync(Guid id, UpdateMedicineDto medicineDto, Guid userId);
    Task<bool> DeleteMedicineAsync(Guid id, Guid userId);
}
