using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Settings;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// Real API implementation of ISettingsRepository
/// Wraps ISettingsApi (Refit) and transforms DTOs to Result<T>
/// Handles HTTP errors with user-friendly messages
/// Automatically refreshes settings after updates
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly ISettingsApi _settingsApi;

    public SettingsRepository(ISettingsApi settingsApi)
    {
        _settingsApi = settingsApi ?? throw new ArgumentNullException(nameof(settingsApi));
    }

    /// <summary>
    /// Get current application settings from server
    /// </summary>
    public async Task<Result<SettingsResponse>> GetSettingsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SettingsRepository.GetSettingsAsync] Calling API to get settings");

            var response = await _settingsApi.GetSettingsAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                // Unwrap ApiResponse<SettingsResponse> to get SettingsResponse
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var settings = apiResponse.Result;
                    System.Diagnostics.Debug.WriteLine($"[SettingsRepository.GetSettingsAsync] ✅ Success: ShopName={settings.ShopName}, Address={settings.Address}");
                    return Result<SettingsResponse>.Success(settings);
                }
                else
                {
                    var errorMessage = apiResponse.Message ?? "Failed to load settings";
                    System.Diagnostics.Debug.WriteLine($"[SettingsRepository.GetSettingsAsync] ❌ Failed: {errorMessage}");
                    return Result<SettingsResponse>.Failure(errorMessage);
                }
            }
            else
            {
                var errorMessage = response.Error?.Message ?? "Failed to load settings";
                System.Diagnostics.Debug.WriteLine($"[SettingsRepository.GetSettingsAsync] ❌ Failed: {errorMessage}");
                return Result<SettingsResponse>.Failure(errorMessage);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsRepository.GetSettingsAsync] ❌ Error: {ex.Message}");
            return Result<SettingsResponse>.Failure($"An error occurred while loading settings: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Update application settings (Admin only)
    /// Automatically calls GetSettingsAsync() to refresh data after successful update
    /// </summary>
    public async Task<Result<SettingsResponse>> UpdateSettingsAsync(UpdateSettingsRequest request)
    {
        try
        {
            if (request == null)
            {
                return Result<SettingsResponse>.Failure("Settings request cannot be null");
            }

            System.Diagnostics.Debug.WriteLine("[SettingsRepository.UpdateSettingsAsync] Calling API to update settings (Admin)");

            var response = await _settingsApi.UpdateSettingsAsync(request);

            if (response.IsSuccessStatusCode && response.Content?.Success == true)
            {
                System.Diagnostics.Debug.WriteLine("[SettingsRepository.UpdateSettingsAsync] ✅ Update successful, refreshing settings");

                // Refresh settings after successful update
                var refreshResult = await GetSettingsAsync();
                if (refreshResult.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine("[SettingsRepository.UpdateSettingsAsync] ✅ Settings refreshed after update");
                    return refreshResult;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SettingsRepository.UpdateSettingsAsync] ⚠️ Update successful but refresh failed");
                    return refreshResult;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorMessage = "Only Admin can update these settings";
                System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateSettingsAsync] ❌ Forbidden: {errorMessage}");
                return Result<SettingsResponse>.Failure(errorMessage);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Try to extract error message from response body
                string errorMessage = "Invalid settings data";
                if (response.Content?.Message != null)
                {
                    errorMessage = response.Content.Message;
                }
                else if (response.Error?.Message != null)
                {
                    errorMessage = response.Error.Message;
                }

                System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateSettingsAsync] ❌ Validation Error: {errorMessage}");
                return Result<SettingsResponse>.Failure(errorMessage);
            }
            else
            {
                var errorMessage = response.Content?.Message ?? response.Error?.Message ?? "Failed to update settings";
                System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateSettingsAsync] ❌ Failed: {errorMessage}");
                return Result<SettingsResponse>.Failure(errorMessage);
            }
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateSettingsAsync] ❌ Network Error: {ex.Message}");
            return Result<SettingsResponse>.Failure($"Network error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateSettingsAsync] ❌ Error: {ex.Message}");
            return Result<SettingsResponse>.Failure($"An error occurred while updating settings: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Update appearance settings (SalesAgent and User only)
    /// Automatically calls GetSettingsAsync() to refresh data after successful update
    /// </summary>
    public async Task<Result<SettingsResponse>> UpdateAppearanceAsync(UpdateAppearanceRequest request)
    {
        try
        {
            if (request == null)
            {
                return Result<SettingsResponse>.Failure("Appearance request cannot be null");
            }

            System.Diagnostics.Debug.WriteLine("[SettingsRepository.UpdateAppearanceAsync] Calling API to update appearance (SalesAgent/User)");

            var response = await _settingsApi.UpdateAppearanceAsync(request);

            if (response.IsSuccessStatusCode && response.Content?.Success == true)
            {
                System.Diagnostics.Debug.WriteLine("[SettingsRepository.UpdateAppearanceAsync] ✅ Update successful, refreshing settings");

                // Refresh settings after successful update
                var refreshResult = await GetSettingsAsync();
                if (refreshResult.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine("[SettingsRepository.UpdateAppearanceAsync] ✅ Settings refreshed after update");
                    return refreshResult;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[SettingsRepository.UpdateAppearanceAsync] ⚠️ Update successful but refresh failed");
                    return refreshResult;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorMessage = "Only SalesAgent and User can update appearance settings";
                System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateAppearanceAsync] ❌ Forbidden: {errorMessage}");
                return Result<SettingsResponse>.Failure(errorMessage);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Try to extract error message from response body
                string errorMessage = "Invalid appearance data";
                if (response.Content?.Message != null)
                {
                    errorMessage = response.Content.Message;
                }
                else if (response.Error?.Message != null)
                {
                    errorMessage = response.Error.Message;
                }

                System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateAppearanceAsync] ❌ Validation Error: {errorMessage}");
                return Result<SettingsResponse>.Failure(errorMessage);
            }
            else
            {
                var errorMessage = response.Content?.Message ?? response.Error?.Message ?? "Failed to update appearance";
                System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateAppearanceAsync] ❌ Failed: {errorMessage}");
                return Result<SettingsResponse>.Failure(errorMessage);
            }
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateAppearanceAsync] ❌ Network Error: {ex.Message}");
            return Result<SettingsResponse>.Failure($"Network error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsRepository.UpdateAppearanceAsync] ❌ Error: {ex.Message}");
            return Result<SettingsResponse>.Failure($"An error occurred while updating appearance: {ex.Message}", ex);
        }
    }
}
