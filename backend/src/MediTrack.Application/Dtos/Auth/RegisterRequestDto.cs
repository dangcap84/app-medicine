using System.ComponentModel.DataAnnotations;

namespace MediTrack.Application.Dtos.Auth;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)] // Add password complexity requirements as needed
    public string Password { get; set; } = null!;
}
