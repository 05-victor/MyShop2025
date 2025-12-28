using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Shared.Models;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation of IAuthRepository for demo purposes
/// </summary>
public class MockAuthRepository : IAuthRepository
{
    private readonly ICredentialStorage _credentialStorage;

    public MockAuthRepository(ICredentialStorage credentialStorage)
    {
        _credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
    }
    public async Task<Result<User>> LoginAsync(string usernameOrEmail, string password, bool rememberMe = false)
    {
        try
        {
            var result = await MockAuthData.LoginAsync(usernameOrEmail, password);

            // Only save tokens if remember me is enabled
            if (result.IsSuccess && result.Data != null && rememberMe)
            {
                var user = result.Data;
                // Mock data has tokens - save them if rememberMe is true
                if (!string.IsNullOrEmpty(user.Token))
                {
                    var refreshToken = user.Token; // Mock uses same token for simplicity
                    await _credentialStorage.SaveToken(user.Token, refreshToken);
                    System.Diagnostics.Debug.WriteLine($"[MockAuthRepository] Saved tokens (rememberMe=true)");
                }
            }
            else if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[MockAuthRepository] Tokens NOT saved (rememberMe=false)");
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Mock Login Error: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
        }
    }

    public async Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
    {
        try
        {
            return await MockAuthData.RegisterAsync(username, email, phoneNumber, password, role);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Mock Register Error: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
        }
    }

    public async Task<Result<User>> GetCurrentUserAsync()
    {
        try
        {
            var token = _credentialStorage.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Result<User>.Failure("No authentication token found");
            }

            return await MockAuthData.GetCurrentUserAsync(token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Mock GetCurrentUser Error: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
        }
    }

    public async Task<Result<User>> ActivateTrialAsync(string adminCode)
    {
        try
        {
            var token = _credentialStorage.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Result<User>.Failure("No authentication token found");
            }

            return await MockAuthData.ActivateTrialAsync(token, adminCode);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Mock ActivateTrial Error: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
        }
    }

    public async Task<Result<Guid>> GetCurrentUserIdAsync()
    {
        try
        {
            var userResult = await GetCurrentUserAsync();

            if (userResult.IsSuccess && userResult.Data != null)
            {
                return Result<Guid>.Success(userResult.Data.Id);
            }

            return Result<Guid>.Failure(userResult.ErrorMessage ?? "Failed to get current user ID");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Mock GetCurrentUserId Error: {ex.Message}");
            return Result<Guid>.Failure("Failed to get current user ID", ex);
        }
    }

    public async Task<Result<Unit>> SendVerificationEmailAsync(string userId)
    {
        try
        {
            // Simulate sending email
            // await Task.Delay(500);
            System.Diagnostics.Debug.WriteLine($"[Mock] Verification email sent to user {userId}");
            System.Diagnostics.Debug.WriteLine($"[Mock] Verification code: 123456 (for testing)");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Mock SendVerificationEmail Error: {ex.Message}");
            return Result<Unit>.Failure("Failed to send verification email. Please try again.", ex);
        }
    }

    public async Task<Result<bool>> CheckVerificationStatusAsync(string userId)
    {
        try
        {
            // Simulate API call
            // await Task.Delay(300);

            // Check if user exists in mock data and get verification status
            var user = await MockAuthData.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            return Result<bool>.Success(user.IsEmailVerified);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Mock CheckVerificationStatus Error: {ex.Message}");
            return Result<bool>.Failure("Failed to check verification status.", ex);
        }
    }

    public async Task<Result<Unit>> VerifyEmailAsync(string userId, string verificationCode)
    {
        try
        {
            // Simulate API call
            // await Task.Delay(500);

            // For demo: accept "123456" as valid code
            if (verificationCode == "123456")
            {
                // Update user verification status in mock data
                await MockAuthData.UpdateEmailVerificationAsync(userId, true);
                System.Diagnostics.Debug.WriteLine($"[Mock] User {userId} email verified successfully");
                return Result<Unit>.Success(Unit.Value);
            }

            return Result<Unit>.Failure("Invalid verification code");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Mock VerifyEmail Error: {ex.Message}");
            return Result<Unit>.Failure("Failed to verify email. Please try again.", ex);
        }
    }
}
