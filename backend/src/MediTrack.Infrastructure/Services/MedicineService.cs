using MediTrack.Application.Dtos.Medicine;
using MediTrack.Application.Interfaces;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediTrack.Infrastructure.Services;

public class MedicineService : IMedicineService
{
    private readonly ApplicationDbContext _context;

    public MedicineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MedicineDto?> GetMedicineByIdAsync(Guid id, Guid userId)
    {
        var medicine = await _context.Medicines
            .Include(m => m.MedicineUnit) // Include related MedicineUnit
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medicine == null) return null;

        return MapToDto(medicine);
    }

    public async Task<IEnumerable<MedicineDto>> GetMedicinesByUserIdAsync(Guid userId)
    {
        var medicines = await _context.Medicines
            .Include(m => m.MedicineUnit)
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Name) // Order by name for consistency
            .ToListAsync();

        return medicines.Select(MapToDto);
    }

    public async Task<MedicineDto?> CreateMedicineAsync(CreateMedicineDto medicineDto, Guid userId)
    {
        // Optional: Validate MedicineUnitId exists
        var unitExists = await _context.MedicineUnits.AnyAsync(u => u.Id == medicineDto.MedicineUnitId);
        if (!unitExists)
        {
            // Handle error: MedicineUnit not found
            return null;
        }

        var medicine = new Medicine
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = medicineDto.Name,
            Dosage = medicineDto.Dosage,
            MedicineUnitId = medicineDto.MedicineUnitId,
            Notes = medicineDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();

        // Need to load the MedicineUnit for the DTO
        await _context.Entry(medicine).Reference(m => m.MedicineUnit).LoadAsync();

        return MapToDto(medicine);
    }

    public async Task<bool> UpdateMedicineAsync(Guid id, UpdateMedicineDto medicineDto, Guid userId)
    {
        var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medicine == null) return false;

        // Optional: Validate MedicineUnitId exists
        var unitExists = await _context.MedicineUnits.AnyAsync(u => u.Id == medicineDto.MedicineUnitId);
        if (!unitExists)
        {
            // Handle error: MedicineUnit not found
            return false;
        }

        medicine.Name = medicineDto.Name;
        medicine.Dosage = medicineDto.Dosage;
        medicine.MedicineUnitId = medicineDto.MedicineUnitId;
        medicine.Notes = medicineDto.Notes;
        medicine.UpdatedAt = DateTime.UtcNow;

        _context.Medicines.Update(medicine);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteMedicineAsync(Guid id, Guid userId)
    {
        var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medicine == null) return false;

        _context.Medicines.Remove(medicine);
        await _context.SaveChangesAsync();

        return true;
    }

    // Helper method to map Entity to DTO
    private static MedicineDto MapToDto(Medicine medicine)
    {
        return new MedicineDto
        {
            Id = medicine.Id,
            UserId = medicine.UserId,
            Name = medicine.Name,
            Dosage = medicine.Dosage,
            MedicineUnitId = medicine.MedicineUnitId,
            MedicineUnitName = medicine.MedicineUnit?.Name ?? "N/A", // Handle potential null if Include fails
            Notes = medicine.Notes,
            CreatedAt = medicine.CreatedAt,
            UpdatedAt = medicine.UpdatedAt
        };
    }
}
