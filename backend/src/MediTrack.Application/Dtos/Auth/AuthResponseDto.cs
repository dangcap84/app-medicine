namespace MediTrack.Application.Dtos.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
}
