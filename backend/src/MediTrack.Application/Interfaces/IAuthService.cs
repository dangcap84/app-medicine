using MediTrack.Application.Dtos.Auth;
using System.Threading.Tasks;

namespace MediTrack.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginDto);
}
