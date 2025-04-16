using MediTrack.Application.Dtos.Schedule;
using MediTrack.Application.Interfaces;
using MediTrack.Domain.Entities;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediTrack.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    private readonly ApplicationDbContext _context;

    public ScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleDto?> GetScheduleByIdAsync(Guid id, Guid userId)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Medicine) // Include Medicine for name
            .Include(s => s.ScheduleTimes) // Include related times
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (schedule == null) return null;

        return MapToDto(schedule);
    }

    public async Task<IEnumerable<ScheduleDto>> GetSchedulesByUserIdAsync(Guid userId)
    {
        var schedules = await _context.Schedules
            .Include(s => s.Medicine)
            .Include(s => s.ScheduleTimes)
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.StartDate)
            .ThenBy(s => s.Medicine.Name)
            .ToListAsync();

        return schedules.Select(MapToDto);
    }

     public async Task<IEnumerable<ScheduleDto>> GetSchedulesByMedicineIdAsync(Guid medicineId, Guid userId)
    {
        var schedules = await _context.Schedules
            .Include(s => s.Medicine)
            .Include(s => s.ScheduleTimes)
            .Where(s => s.MedicineId == medicineId && s.UserId == userId)
            .OrderBy(s => s.StartDate)
            .ToListAsync();

        return schedules.Select(MapToDto);
    }

    public async Task<ScheduleDto?> CreateScheduleAsync(CreateScheduleDto scheduleDto, Guid userId)
    {
        // Validate MedicineId belongs to the user
        var medicineExists = await _context.Medicines.AnyAsync(m => m.Id == scheduleDto.MedicineId && m.UserId == userId);
        if (!medicineExists)
        {
            // Handle error: Medicine not found or doesn't belong to user
            return null;
        }

        // Validate DaysOfWeek based on FrequencyType
        if (scheduleDto.FrequencyType == FrequencyType.Weekly && string.IsNullOrWhiteSpace(scheduleDto.DaysOfWeek))
        {
             return null; // Or throw validation exception
        }
        if (scheduleDto.FrequencyType != FrequencyType.Weekly && !string.IsNullOrWhiteSpace(scheduleDto.DaysOfWeek))
        {
            // Clear DaysOfWeek if frequency is not weekly to avoid inconsistent data
            scheduleDto.DaysOfWeek = null;
        }


        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MedicineId = scheduleDto.MedicineId,
            StartDate = scheduleDto.StartDate.ToUniversalTime(), // Store in UTC
            EndDate = scheduleDto.EndDate?.ToUniversalTime(), // Store in UTC
            FrequencyType = scheduleDto.FrequencyType,
            DaysOfWeek = scheduleDto.DaysOfWeek,
            Notes = scheduleDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ScheduleTimes = scheduleDto.ScheduleTimes.Select(t => new ScheduleTime
            {
                Id = Guid.NewGuid(),
                TimeOfDay = t.TimeOfDay,
                Quantity = t.Quantity
            }).ToList()
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        // Need to load Medicine for the DTO
        await _context.Entry(schedule).Reference(s => s.Medicine).LoadAsync();

        return MapToDto(schedule);
    }

    public async Task<bool> UpdateScheduleAsync(Guid id, UpdateScheduleDto scheduleDto, Guid userId)
    {
        var schedule = await _context.Schedules
            .Include(s => s.ScheduleTimes) // Include existing times to remove them
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (schedule == null) return false;

        // Validate DaysOfWeek based on FrequencyType
        if (scheduleDto.FrequencyType == FrequencyType.Weekly && string.IsNullOrWhiteSpace(scheduleDto.DaysOfWeek))
        {
             return false; // Or throw validation exception
        }
         if (scheduleDto.FrequencyType != FrequencyType.Weekly && !string.IsNullOrWhiteSpace(scheduleDto.DaysOfWeek))
        {
            scheduleDto.DaysOfWeek = null;
        }

        // Update schedule properties
        schedule.StartDate = scheduleDto.StartDate.ToUniversalTime();
        schedule.EndDate = scheduleDto.EndDate?.ToUniversalTime();
        schedule.FrequencyType = scheduleDto.FrequencyType;
        schedule.DaysOfWeek = scheduleDto.DaysOfWeek;
        schedule.Notes = scheduleDto.Notes;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Remove existing times
        _context.ScheduleTimes.RemoveRange(schedule.ScheduleTimes);

        // Add new times
        schedule.ScheduleTimes = scheduleDto.ScheduleTimes.Select(t => new ScheduleTime
        {
            Id = Guid.NewGuid(),
            ScheduleId = schedule.Id, // Explicitly set ScheduleId
            TimeOfDay = t.TimeOfDay,
            Quantity = t.Quantity
        }).ToList();

        // _context.Schedules.Update(schedule); // Update is often implicit when tracking changes
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteScheduleAsync(Guid id, Guid userId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (schedule == null) return false;

        // Related ScheduleTimes will be deleted due to cascade delete constraint
        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return true;
    }

    // Helper method to map Entity to DTO
    private static ScheduleDto MapToDto(Schedule schedule)
    {
        return new ScheduleDto
        {
            Id = schedule.Id,
            UserId = schedule.UserId,
            MedicineId = schedule.MedicineId,
            MedicineName = schedule.Medicine?.Name ?? "N/A", // Handle potential null
            StartDate = schedule.StartDate,
            EndDate = schedule.EndDate,
            FrequencyType = schedule.FrequencyType,
            DaysOfWeek = schedule.DaysOfWeek,
            Notes = schedule.Notes,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt,
            ScheduleTimes = schedule.ScheduleTimes?.Select(t => new ScheduleTimeDto
            {
                Id = t.Id,
                TimeOfDay = t.TimeOfDay,
                Quantity = t.Quantity
            }).ToList() ?? new List<ScheduleTimeDto>() // Handle potential null
        };
    }
}
