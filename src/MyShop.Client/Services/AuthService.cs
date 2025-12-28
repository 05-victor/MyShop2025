using MyShop.Core.Common;
using MyShop.Core.Interfaces.Services;

namespace MyShop.Client.Services;

/// <summary>
/// Stub implementation of IAuthService for password reset operations.
/// TODO: Implement actual API calls when backend is ready.
/// </summary>
public class AuthService : IAuthService
{
    public AuthService()
    {
    }

    /// <summary>
    /// Send a password reset code to the specified email.
    /// TODO: Call actual API endpoint.
    /// </summary>
    public async Task<Result<Unit>> SendPasswordResetCodeAsync(string email)
    {
        // Simulate network delay
        await Task.Delay(1000);

        // TODO: Replace with actual API call
        // For now, simulate success
        return Result<Unit>.Success(Unit.Value);

        // Example error cases to implement:
        // return Result<Unit>.Failure("Email not found");
        // return Result<Unit>.Failure("Network error");
    }

    /// <summary>
    /// Verify a password reset code for the specified email.
    /// TODO: Call actual API endpoint.
    /// </summary>
    public async Task<Result<string>> VerifyPasswordResetCodeAsync(string email, string code)
    {
        // Simulate network delay
        await Task.Delay(1000);

        // TODO: Replace with actual API call
        // For now, simulate success with a mock token
        string mockToken = $"reset_token_{Guid.NewGuid():N}";
        return Result<string>.Success(mockToken);

        // Example error cases to implement:
        // return Result<string>.Failure("Invalid code");
        // return Result<string>.Failure("Code expired");
        // return Result<string>.Failure("Too many attempts");
    }

    /// <summary>
    /// Reset the password using the provided token.
    /// TODO: Call actual API endpoint.
    /// </summary>
    public async Task<Result<Unit>> ResetPasswordAsync(string email, string token, string newPassword)
    {
        // Simulate network delay
        await Task.Delay(1000);

        // TODO: Replace with actual API call
        // For now, simulate success
        return Result<Unit>.Success(Unit.Value);

        // Example error cases to implement:
        // return Result<Unit>.Failure("Invalid or expired token");
        // return Result<Unit>.Failure("Password does not meet requirements");
    }
}
