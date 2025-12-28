using MyShop.Core.Common;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Auth;
using MyShop.Plugins.Adapters;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;
using Refit;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// Real API implementation of IAuthRepository.
/// Wraps IAuthApi (Refit) and transforms DTOs to Client Models.
/// Handles HTTP errors and network exceptions with user-friendly messages.
/// </summary>
public class AuthRepository : IAuthRepository
{
    private readonly IAuthApi _authApi;
    private readonly ICredentialStorage _credentialStorage;

    public AuthRepository(IAuthApi authApi, ICredentialStorage credentialStorage)
    {
        _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
        _credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
    }

    public async Task<Result<User>> LoginAsync(string usernameOrEmail, string password, bool rememberMe = false)
    {
        try
        {
            var request = new LoginRequest
            {
                UsernameOrEmail = usernameOrEmail,
                Password = password
            };

            var refitResponse = await _authApi.LoginAsync(request);

            // Check Refit outer wrapper (HTTP status)
            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;

                // Check inner ApiResponse (business logic)
                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    // Store refresh token temporarily so we can pass it to AuthFacade
                    var loginResponse = apiResponse.Result;

                    // Only save tokens if remember me is enabled
                    if (rememberMe && !string.IsNullOrEmpty(loginResponse.RefreshToken))
                    {
                        await _credentialStorage.SaveToken(loginResponse.Token, loginResponse.RefreshToken);
                        System.Diagnostics.Debug.WriteLine($"[AuthRepository] Saved access and refresh tokens (rememberMe=true)");
                    }
                    else if (rememberMe)
                    {
                        await _credentialStorage.SaveToken(loginResponse.Token);
                        System.Diagnostics.Debug.WriteLine($"[AuthRepository] Saved access token only (rememberMe=true, no refresh token from server)");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[AuthRepository] Tokens NOT saved (rememberMe=false)");
                    }

                    // Transform DTO → Client Model using AuthAdapter
                    var user = AuthAdapter.ToModel(loginResponse);
                    return Result<User>.Success(user);
                }

                return Result<User>.Failure(apiResponse.Message ?? "Login failed");
            }

            // HTTP error
            return Result<User>.Failure($"HTTP Error: {refitResponse.StatusCode}");
        }
        catch (ApiException apiEx)
        {
            var errorMessage = MapApiError(apiEx);
            return Result<User>.Failure(errorMessage, apiEx);
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
            return Result<User>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
        }
    }

    public async Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
    {
        try
        {
            var request = new CreateUserRequest
            {
                Username = username,
                Email = email,
                Password = password,
                Sdt = phoneNumber,
                ActivateTrial = false,
                RoleNames = new List<string> { role }
            };

            var refitResponse = await _authApi.RegisterAsync(request);

            // Check Refit outer wrapper (HTTP status)
            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;

                // Check inner ApiResponse (business logic)
                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    // Note: Register response doesn't include token, need to login after
                    var user = new User
                    {
                        Id = apiResponse.Result.Id,
                        Username = username,
                        Email = email,
                        CreatedAt = DateTime.Now
                    };
                    return Result<User>.Success(user);
                }

                return Result<User>.Failure(apiResponse.Message ?? "Registration failed");
            }

            // HTTP error
            return Result<User>.Failure($"HTTP Error: {refitResponse.StatusCode}");
        }
        catch (ApiException apiEx)
        {
            var errorMessage = MapApiError(apiEx);
            return Result<User>.Failure(errorMessage, apiEx);
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
            return Result<User>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
        }
    }

    public async Task<Result<User>> GetCurrentUserAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AuthRepository.GetCurrentUserAsync] Calling GetMeAsync()...");
            var refitResponse = await _authApi.GetMeAsync();

            System.Diagnostics.Debug.WriteLine($"[AuthRepository.GetCurrentUserAsync] GetMeAsync response: StatusCode={refitResponse.StatusCode}, IsSuccess={refitResponse.IsSuccessStatusCode}");

            // Check Refit outer wrapper (HTTP status)
            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;

                System.Diagnostics.Debug.WriteLine($"[AuthRepository.GetCurrentUserAsync] API response: Success={apiResponse.Success}, Message={apiResponse.Message}");

                // Check inner ApiResponse (business logic)
                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AuthRepository.GetCurrentUserAsync] ✓ User data received from GetMe");
                    var token = _credentialStorage.GetToken() ?? string.Empty;
                    var user = AuthAdapter.ToModel(apiResponse.Result, token);
                    return Result<User>.Success(user);
                }

                System.Diagnostics.Debug.WriteLine($"[AuthRepository.GetCurrentUserAsync] ✗ GetMe returned no data or success=false");
                return Result<User>.Failure(apiResponse.Message ?? "Failed to get user info");
            }

            // HTTP error
            System.Diagnostics.Debug.WriteLine($"[AuthRepository.GetCurrentUserAsync] ✗ HTTP Error: {refitResponse.StatusCode}");
            return Result<User>.Failure($"HTTP Error: {refitResponse.StatusCode}");
        }
        catch (ApiException apiEx)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthRepository.GetCurrentUserAsync] ✗ ApiException: {apiEx.Message}");
            var errorMessage = MapApiError(apiEx);
            return Result<User>.Failure(errorMessage, apiEx);
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthRepository.GetCurrentUserAsync] ✗ Network Error: {httpEx.Message}");
            return Result<User>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"GetCurrentUserId Error: {ex.Message}");
            return Result<Guid>.Failure("Failed to get current user ID", ex);
        }
    }

    public async Task<Result<User>> ActivateTrialAsync(string adminCode)
    {
        try
        {
            // TODO: Implement real API call when backend endpoint is ready
            // For now, return not implemented
            // await Task.Delay(500); // Simulate network delay

            return Result<User>.Failure("Trial activation API not yet implemented on server. Please use mock mode for testing.");

            /*
            // Future implementation when backend is ready:
            var request = new ActivateTrialRequest { AdminCode = adminCode };
            var refitResponse = await _authApi.ActivateTrialAsync(request);

            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;

                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    var token = _credentialStorage.GetToken() ?? string.Empty;
                    var user = AuthAdapter.ToModel(apiResponse.Result, token);
                    return Result<User>.Success(user);
                }

                return Result<User>.Failure(apiResponse.Message ?? "Trial activation failed");
            }

            return Result<User>.Failure($"HTTP Error: {refitResponse.StatusCode}");
            */
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ActivateTrial Error: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
        }
    }

    public async Task<Result<Unit>> SendVerificationEmailAsync(string userId)
    {
        try
        {
            // await Task.Delay(500);
            return Result<Unit>.Failure("Email verification API not yet implemented on server. Please use mock mode for testing.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SendVerificationEmail Error: {ex.Message}");
            return Result<Unit>.Failure("An unexpected error occurred.", ex);
        }
    }

    public async Task<Result<bool>> CheckVerificationStatusAsync(string userId)
    {
        try
        {
            // await Task.Delay(500);
            return Result<bool>.Failure("Email verification status API not yet implemented on server. Please use mock mode for testing.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckVerificationStatus Error: {ex.Message}");
            return Result<bool>.Failure("An unexpected error occurred.", ex);
        }
    }

    public async Task<Result<Unit>> VerifyEmailAsync(string userId, string verificationCode)
    {
        try
        {
            // await Task.Delay(500);
            return Result<Unit>.Failure("Email verification API not yet implemented on server. Please use mock mode for testing.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"VerifyEmail Error: {ex.Message}");
            return Result<Unit>.Failure("An unexpected error occurred.", ex);
        }
    }

    // DTO Mapping methods removed - now using AuthAdapter

    #region Error Handling

    /// <summary>
    /// Map API errors to user-friendly messages.
    /// </summary>
    private string MapApiError(ApiException apiEx)
    {
        System.Diagnostics.Debug.WriteLine($"API Error: {apiEx.StatusCode} - {apiEx.Content}");

        return apiEx.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Invalid username or password",
            System.Net.HttpStatusCode.NotFound => "Account not found",
            System.Net.HttpStatusCode.BadRequest => ParseBadRequestError(apiEx.Content),
            _ => "Network error. Please check your connection"
        };
    }

    private string ParseBadRequestError(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return "Invalid request";

        if (content.Contains("username", StringComparison.OrdinalIgnoreCase))
            return "Username already exists";

        if (content.Contains("email", StringComparison.OrdinalIgnoreCase))
            return "Email already registered";

        return "Invalid request data";
    }

    #endregion
}
