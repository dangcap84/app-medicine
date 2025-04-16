using MediTrack.Application.Dtos.Schedule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediTrack.Application.Interfaces;

public interface IScheduleService
{
    Task<ScheduleDto?> GetScheduleByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<ScheduleDto>> GetSchedulesByUserIdAsync(Guid userId);
    Task<IEnumerable<ScheduleDto>> GetSchedulesByMedicineIdAsync(Guid medicineId, Guid userId);
    Task<ScheduleDto?> CreateScheduleAsync(CreateScheduleDto scheduleDto, Guid userId);
    Task<bool> UpdateScheduleAsync(Guid id, UpdateScheduleDto scheduleDto, Guid userId);
    Task<bool> DeleteScheduleAsync(Guid id, Guid userId);
}
