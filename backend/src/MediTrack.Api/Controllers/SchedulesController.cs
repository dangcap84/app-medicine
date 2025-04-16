using MediTrack.Application.Dtos.Schedule;
using MediTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MediTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    // Helper to get current UserId from JWT token
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token.");
        }
        return userId;
    }

    // GET: api/Schedules
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedules()
    {
        var userId = GetCurrentUserId();
        var schedules = await _scheduleService.GetSchedulesByUserIdAsync(userId);
        return Ok(schedules);
    }

    // GET: api/Schedules/medicine/{medicineId}
    [HttpGet("medicine/{medicineId}")]
    public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedulesForMedicine(Guid medicineId)
    {
        var userId = GetCurrentUserId();
        var schedules = await _scheduleService.GetSchedulesByMedicineIdAsync(medicineId, userId);
        return Ok(schedules);
    }


    // GET: api/Schedules/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduleDto>> GetSchedule(Guid id)
    {
        var userId = GetCurrentUserId();
        var schedule = await _scheduleService.GetScheduleByIdAsync(id, userId);

        if (schedule == null)
        {
            return NotFound();
        }

        return Ok(schedule);
    }

    // POST: api/Schedules
    [HttpPost]
    public async Task<ActionResult<ScheduleDto>> CreateSchedule([FromBody] CreateScheduleDto createScheduleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var createdSchedule = await _scheduleService.CreateScheduleAsync(createScheduleDto, userId);

        if (createdSchedule == null)
        {
            // Could be due to invalid MedicineId or other validation errors in the service
            return BadRequest("Failed to create schedule. Check input data (e.g., MedicineId, DaysOfWeek).");
        }

        return CreatedAtAction(nameof(GetSchedule), new { id = createdSchedule.Id }, createdSchedule);
    }

    // PUT: api/Schedules/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] UpdateScheduleDto updateScheduleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var success = await _scheduleService.UpdateScheduleAsync(id, updateScheduleDto, userId);

        if (!success)
        {
            // Could be NotFound or other validation error
            return NotFound("Schedule not found or update failed.");
        }

        return NoContent();
    }

    // DELETE: api/Schedules/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        var userId = GetCurrentUserId();
        var success = await _scheduleService.DeleteScheduleAsync(id, userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
