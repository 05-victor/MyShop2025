using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IAuthService
{
    Task<CreateUserResponse> RegisterAsync(CreateUserRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Get user info by user ID
    /// </summary>
    Task<UserInfoResponse?> GetMeAsync(Guid userId);
    
    /// <summary>
    /// Get current authenticated user info from JWT token
    /// </summary>
    Task<UserInfoResponse?> GetMeAsync();
}
