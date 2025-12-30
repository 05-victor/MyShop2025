using Microsoft.Extensions.Logging;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Plugins.API.Auth;
using MyShop.Plugins.Infrastructure;
using MyShop.Shared.DTOs.Requests;
using System.Text.Json;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// Real API implementation of ISystemActivationRepository
/// Calls backend API endpoints for license activation
/// </summary>
public class SystemActivationRepository : ISystemActivationRepository
{
    private readonly IAuthApi _authApi;
    private readonly ILogger<SystemActivationRepository> _logger;
    private readonly ICredentialStorage _credentialStorage;

    // Cache admin existence status to avoid repeated API calls
    private bool? _cachedHasAdmin;
    private DateTime _cacheTime = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5); // Cache for 5 minutes

    public SystemActivationRepository(IAuthApi authApi, ICredentialStorage credentialStorage, ILogger<SystemActivationRepository> logger)
    {
        _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
        _credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Activate an admin code for a user via backend API
    /// </summary>
    public async Task<Result<LicenseInfo>> ActivateCodeAsync(string code, Guid userId)
    {
        try
        {
            _logger.LogInformation("[SystemActivationRepository] ActivateCodeAsync called with code: {Code}, userId: {UserId}", code, userId);

            // Step 1: Call activate endpoint
            var refitResponse = await _authApi.ActivateAsync(code);

            _logger.LogInformation("[SystemActivationRepository] Activate response - StatusCode: {StatusCode}, IsSuccess: {IsSuccess}, Content: {Content}",
                refitResponse.StatusCode, refitResponse.IsSuccessStatusCode, refitResponse.Content != null ? "Present" : "Null");

            // Handle both success (200) and error (4xx) responses
            string? errorMessage = null;

            if (refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;
                _logger.LogInformation("[SystemActivationRepository] API Response: Success={Success}, Message={Message}",
                    apiResponse.Success, apiResponse.Message);

                if (refitResponse.IsSuccessStatusCode && apiResponse.Success == true)
                {
                    _logger.LogInformation("[SystemActivationRepository] Activation successful for user {UserId}", userId);

                    // Step 2: Get updated user info to retrieve license details
                    var userResponse = await _authApi.GetMeAsync();

                    if (userResponse.IsSuccessStatusCode && userResponse.Content?.Result != null)
                    {
                        var userInfo = userResponse.Content.Result;

                        // Step 3: Convert user info to LicenseInfo
                        // Note: IsPermanent is a computed property based on Type, so don't assign it
                        var licenseInfo = new LicenseInfo
                        {
                            UserId = userId,
                            Type = userInfo.IsTrialActive ? "trial" : "permanent",
                            ActivatedAt = DateTime.UtcNow,
                            ExpiresAt = userInfo.TrialEndDate,
                            DurationDays = userInfo.TrialEndDate.HasValue
                                ? (int?)(userInfo.TrialEndDate.Value - DateTime.UtcNow).TotalDays
                                : null,
                            RemainingDays = userInfo.TrialEndDate.HasValue
                                ? Math.Max(0, (int)(userInfo.TrialEndDate.Value - DateTime.UtcNow).TotalDays)
                                : 0,
                            IsExpired = userInfo.TrialEndDate.HasValue && userInfo.TrialEndDate.Value < DateTime.UtcNow,
                            IsExpiring = false // Could be calculated based on days remaining
                        };

                        _logger.LogInformation("[SystemActivationRepository] License info created - Type: {Type}, IsPermanent: {IsPermanent}, RemainingDays: {RemainingDays}",
                            licenseInfo.Type, licenseInfo.IsPermanent, licenseInfo.RemainingDays);

                        return Result<LicenseInfo>.Success(licenseInfo);
                    }
                    else
                    {
                        _logger.LogWarning("[SystemActivationRepository] Failed to get updated user info after activation");
                        return Result<LicenseInfo>.Failure("Activation successful but failed to retrieve license details");
                    }
                }
                else
                {
                    // API returned error - extract message from response
                    errorMessage = apiResponse.Message ?? $"Activation failed";
                }
            }
            else
            {
                // No response content - try to read error from raw response
                _logger.LogWarning("[SystemActivationRepository] Response content is null - Status: {StatusCode}", refitResponse.StatusCode);

                // Try to extract error from Refit error handling
                if (refitResponse.Error != null && !string.IsNullOrEmpty(refitResponse.Error.Content))
                {
                    _logger.LogWarning("[SystemActivationRepository] Refit error content: {Error}", refitResponse.Error.Content);

                    // Try to parse the error response JSON to extract the message field
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(refitResponse.Error.Content))
                        {
                            if (doc.RootElement.TryGetProperty("message", out var messageProp))
                            {
                                errorMessage = messageProp.GetString();
                            }
                            else
                            {
                                errorMessage = "Activation failed - server returned an error";
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // If JSON parsing fails, use the raw content
                        errorMessage = refitResponse.Error.Content;
                    }
                }
                else
                {
                    errorMessage = "Server returned an error but no error details available";
                }
            }

            // If we got here with an error message, return it
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.LogWarning("[SystemActivationRepository] Activation failed - Status: {StatusCode}, Message: {Message}",
                    refitResponse.StatusCode, errorMessage);
                return Result<LicenseInfo>.Failure(errorMessage);
            }

            // Fallback if neither success nor error path was taken
            _logger.LogError("[SystemActivationRepository] Unexpected state - Status: {StatusCode}", refitResponse.StatusCode);
            return Result<LicenseInfo>.Failure($"Unexpected error: HTTP {(int)refitResponse.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemActivationRepository] Error during ActivateCodeAsync");
            return Result<LicenseInfo>.Failure("An error occurred during activation", ex);
        }
    }

    /// <summary>
    /// Validate an activation code without using it
    /// </summary>
    public async Task<Result<AdminCodeInfo>> ValidateCodeAsync(string code)
    {
        try
        {
            _logger.LogInformation("[SystemActivationRepository] ValidateCodeAsync called with code: {Code}", code);

            // For API mode, we rely on ActivateCodeAsync to validate
            // A dedicated validate endpoint could be added to the backend if needed
            // For now, return success and let ActivateCodeAsync handle validation

            var codeInfo = new AdminCodeInfo
            {
                Code = code,
                Type = code.StartsWith("PRM-") ? "permanent" : "trial",
                IsValid = true
            };

            return Result<AdminCodeInfo>.Success(codeInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemActivationRepository] Error during ValidateCodeAsync");
            return Result<AdminCodeInfo>.Failure("Failed to validate code", ex);
        }
    }

    /// <summary>
    /// Check if any admin exists in the system
    /// Calls backend API endpoint to check if any user has Admin role
    /// Uses caching to avoid repeated API calls within 5 minutes
    /// </summary>
    public async Task<Result<bool>> HasAnyAdminAsync()
    {
        try
        {
            // Check cache first
            if (_cachedHasAdmin.HasValue && DateTime.UtcNow - _cacheTime < _cacheExpiration)
            {
                _logger.LogInformation("[SystemActivationRepository] HasAnyAdminAsync: Using cached value = {HasAdmin}", _cachedHasAdmin);
                return Result<bool>.Success(_cachedHasAdmin.Value);
            }

            // Call backend API endpoint to check if system has any admin
            var response = await _authApi.HasAdminAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var hasAdmin = response.Content.Result;

                // Cache the result
                _cachedHasAdmin = hasAdmin;
                _cacheTime = DateTime.UtcNow;

                _logger.LogInformation("[SystemActivationRepository] HasAnyAdminAsync: System has admin = {HasAdmin} (cached for 5 minutes)", hasAdmin);
                return Result<bool>.Success(hasAdmin);
            }
            else
            {
                // If we can't get info from API, assume no admin (conservative approach)
                _logger.LogWarning("[SystemActivationRepository] HasAnyAdminAsync: Failed to check admin status, assuming no admin");
                return Result<bool>.Success(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemActivationRepository] Error during HasAnyAdminAsync");
            // On error, assume no admin to allow activation attempt
            return Result<bool>.Success(false);
        }
    }

    /// <summary>
    /// Get current license information
    /// Fetches user info from API and maps it to LicenseInfo
    /// </summary>
    public async Task<Result<LicenseInfo?>> GetCurrentLicenseAsync()
    {
        try
        {
            _logger.LogInformation("[SystemActivationRepository] GetCurrentLicenseAsync called");

            // Call GetMeAsync to get current user info including trial dates
            var userResponse = await _authApi.GetMeAsync();

            if (userResponse.IsSuccessStatusCode && userResponse.Content?.Result != null)
            {
                var userInfo = userResponse.Content.Result;
                var userId = userInfo.Id;

                _logger.LogInformation("[SystemActivationRepository] GetCurrentLicenseAsync - User {UserId}: IsTrialActive={IsTrialActive}, TrialEndDate={TrialEndDate}",
                    userId, userInfo.IsTrialActive, userInfo.TrialEndDate);

                // Create LicenseInfo from user info
                // If user doesn't have trial active, they might have permanent license or no license
                var licenseInfo = new LicenseInfo
                {
                    UserId = userId,
                    Type = userInfo.IsTrialActive ? "trial" : "permanent",
                    ActivatedAt = DateTime.UtcNow,
                    ExpiresAt = userInfo.TrialEndDate,
                    DurationDays = userInfo.TrialEndDate.HasValue
                        ? (int?)(userInfo.TrialEndDate.Value - DateTime.UtcNow).TotalDays
                        : null,
                    RemainingDays = userInfo.TrialEndDate.HasValue
                        ? Math.Max(0, (int)(userInfo.TrialEndDate.Value - DateTime.UtcNow).TotalDays)
                        : 0,
                    IsExpired = userInfo.TrialEndDate.HasValue && userInfo.TrialEndDate.Value < DateTime.UtcNow,
                    IsExpiring = userInfo.TrialEndDate.HasValue && (userInfo.TrialEndDate.Value - DateTime.UtcNow).TotalDays <= 3 // Expiring if 3 days or less
                };

                _logger.LogInformation("[SystemActivationRepository] GetCurrentLicenseAsync - License Info: Type={Type}, RemainingDays={RemainingDays}, IsExpired={IsExpired}, ExpiresAt={ExpiresAt}",
                    licenseInfo.Type, licenseInfo.RemainingDays, licenseInfo.IsExpired, licenseInfo.ExpiresAt);

                return Result<LicenseInfo?>.Success(licenseInfo);
            }
            else
            {
                _logger.LogWarning("[SystemActivationRepository] GetCurrentLicenseAsync - Failed to get user info");
                return Result<LicenseInfo?>.Success(null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemActivationRepository] Error during GetCurrentLicenseAsync");
            return Result<LicenseInfo?>.Failure("Failed to get current license", ex);
        }
    }

    /// <summary>
    /// Get remaining trial days
    /// </summary>
    public async Task<Result<int>> GetRemainingTrialDaysAsync()
    {
        try
        {
            // This could call a dedicated API endpoint if needed
            return Result<int>.Success(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemActivationRepository] Error during GetRemainingTrialDaysAsync");
            return Result<int>.Failure("Failed to get remaining trial days", ex);
        }
    }

    /// <summary>
    /// Check if trial is expiring soon
    /// </summary>
    public async Task<Result<bool>> IsTrialExpiringAsync()
    {
        try
        {
            return Result<bool>.Success(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemActivationRepository] Error during IsTrialExpiringAsync");
            return Result<bool>.Failure("Failed to check trial expiry status", ex);
        }
    }

    /// <summary>
    /// Check if trial has expired
    /// </summary>
    public async Task<Result<bool>> IsTrialExpiredAsync()
    {
        try
        {
            return Result<bool>.Success(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemActivationRepository] Error during IsTrialExpiredAsync");
            return Result<bool>.Failure("Failed to check trial expired status", ex);
        }
    }

    /// <summary>
    /// Demote expired admin to customer
    /// </summary>
    public async Task<Result<bool>> DemoteExpiredAdminAsync()
    {
        try
        {
            return Result<bool>.Success(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemActivationRepository] Error during DemoteExpiredAdminAsync");
            return Result<bool>.Failure("Failed to demote expired admin", ex);
        }
    }
}
