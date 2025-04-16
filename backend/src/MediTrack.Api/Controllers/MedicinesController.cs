using MediTrack.Application.Dtos.Medicine;
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
[Authorize] // Require authentication for all actions in this controller
public class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicineService;

    public MedicinesController(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    // Helper to get current UserId from JWT token
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); // Or ClaimTypes.NameIdentifier depending on JWT setup
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            // This should not happen if [Authorize] is working correctly
            throw new UnauthorizedAccessException("User ID not found in token.");
        }
        return userId;
    }

    // GET: api/Medicines
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicineDto>>> GetMedicines()
    {
        var userId = GetCurrentUserId();
        var medicines = await _medicineService.GetMedicinesByUserIdAsync(userId);
        return Ok(medicines);
    }

    // GET: api/Medicines/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<MedicineDto>> GetMedicine(Guid id)
    {
        var userId = GetCurrentUserId();
        var medicine = await _medicineService.GetMedicineByIdAsync(id, userId);

        if (medicine == null)
        {
            return NotFound();
        }

        return Ok(medicine);
    }

    // POST: api/Medicines
    [HttpPost]
    public async Task<ActionResult<MedicineDto>> CreateMedicine([FromBody] CreateMedicineDto createMedicineDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var createdMedicine = await _medicineService.CreateMedicineAsync(createMedicineDto, userId);

        if (createdMedicine == null)
        {
            // Could be due to invalid MedicineUnitId or other service-level validation
            return BadRequest("Failed to create medicine. Check input data (e.g., MedicineUnitId).");
        }

        // Return 201 Created status with location header and the created resource
        return CreatedAtAction(nameof(GetMedicine), new { id = createdMedicine.Id }, createdMedicine);
    }

    // PUT: api/Medicines/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicine(Guid id, [FromBody] UpdateMedicineDto updateMedicineDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var success = await _medicineService.UpdateMedicineAsync(id, updateMedicineDto, userId);

        if (!success)
        {
            // Could be NotFound or other error (like invalid MedicineUnitId)
            return NotFound("Medicine not found or update failed.");
        }

        return NoContent(); // Standard response for successful PUT
    }

    // DELETE: api/Medicines/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicine(Guid id)
    {
        var userId = GetCurrentUserId();
        var success = await _medicineService.DeleteMedicineAsync(id, userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent(); // Standard response for successful DELETE
    }
}
