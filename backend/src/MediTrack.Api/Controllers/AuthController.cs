using MediTrack.Application.Dtos.Auth;
using MediTrack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediTrack.Domain.Exceptions.Authentication;

namespace MediTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try 
        {
            var result = await _authService.RegisterAsync(registerDto);
            return Ok(result);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message, code = ex.Code });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Log the error
            return StatusCode(500, new { message = "An error occurred during authentication.", details = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the error
            return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
        }
    }
}
