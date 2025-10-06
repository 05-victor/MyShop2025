using MyShop.Shared.DTOs.User;

namespace MyShop.Server.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto?> GetMeAsync(int userId);
}
