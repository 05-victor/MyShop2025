using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using MyShop.Plugins.Storage;
using Refit;

namespace MyShop.Plugins.ApiClients.Auth;

/// <summary>
/// Real API implementation của IAuthRepository
/// Wraps IAuthApiClient (Refit) và transform DTOs → Client Models
/// </summary>
public class AuthRepository : IAuthRepository
{
    private readonly IAuthApiClient _authApi;

    public AuthRepository(IAuthApiClient authApi)
    {
        _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
    }

    public async Task<Result<User>> LoginAsync(string usernameOrEmail, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                UsernameOrEmail = usernameOrEmail,
                Password = password
            };

            var response = await _authApi.LoginAsync(request);

            if (response?.Success == true && response.Result != null)
            {
                // Transform DTO → Client Model
                var user = MapLoginResponseToUser(response.Result);
                return Result<User>.Success(user);
            }

            return Result<User>.Failure(response?.Message ?? "Login failed");
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
                Password = password
            };

            var response = await _authApi.RegisterAsync(request);

            if (response?.Success == true && response.Result != null)
            {
                // Note: Register response doesn't include token, need to login after
                var user = new User
                {
                    Id = response.Result.Id,
                    Username = username,
                    Email = email,
                    CreatedAt = DateTime.Now
                };
                return Result<User>.Success(user);
            }

            return Result<User>.Failure(response?.Message ?? "Registration failed");
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
            var response = await _authApi.GetMeAsync();

            if (response?.Success == true && response.Result != null)
            {
                var token = CredentialHelper.GetToken() ?? string.Empty;
                var user = MapUserInfoResponseToUser(response.Result, token);
                return Result<User>.Success(user);
            }

            return Result<User>.Failure(response?.Message ?? "Failed to get user info");
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

    #region DTO Mapping

    /// <summary>
    /// Map LoginResponse (DTO) → User (Client Model)
    /// </summary>
    private User MapLoginResponseToUser(LoginResponse dto)
    {
        return new User
        {
            Id = dto.Id,
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = dto.CreatedAt,
            IsTrialActive = dto.IsTrialActive,
            TrialStartDate = dto.TrialStartDate,
            TrialEndDate = dto.TrialEndDate,
            IsEmailVerified = dto.IsEmailVerified,
            Token = dto.Token,
            Roles = ParseRoles(dto.RoleNames)
        };
    }

    /// <summary>
    /// Map UserInfoResponse (DTO) → User (Client Model)
    /// </summary>
    private User MapUserInfoResponseToUser(UserInfoResponse dto, string token)
    {
        return new User
        {
            Id = dto.Id,
            Username = dto.Username,
            Email = dto.Email,
            CreatedAt = dto.CreatedAt,
            Token = token,
            Roles = ParseRoles(dto.RoleNames)
        };
    }

    /// <summary>
    /// Parse role names (string) → UserRole enums
    /// </summary>
    private List<UserRole> ParseRoles(IEnumerable<string>? roleNames)
    {
        if (roleNames == null)
            return new List<UserRole>();

        var roles = new List<UserRole>();

        foreach (var roleName in roleNames)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                continue;

            var normalized = roleName.Trim().ToUpperInvariant();

            if (normalized == "ADMIN")
                roles.Add(UserRole.Admin);
            else if (normalized == "SALEMAN" || normalized == "SALESMAN")
                roles.Add(UserRole.Salesman);
            else if (normalized == "CUSTOMER")
                roles.Add(UserRole.Customer);
        }

        // Default to Customer if no roles found
        if (roles.Count == 0)
            roles.Add(UserRole.Customer);

        return roles;
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Map API errors thành user-friendly messages
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
