using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Settings;

/// <summary>
/// Refit HTTP client for Settings API endpoints
/// Handles GET and PUT operations for application settings
/// Server wraps responses in ApiResponse<T> envelope
/// </summary>
public interface ISettingsApi
{
    /// <summary>
    /// GET /api/v1/settings
    /// Retrieve current application settings (accessible to all authenticated users)
    /// Returns wrapped in ApiResponse<SettingsResponse>
    /// </summary>
    /// <returns>Current settings including theme, shop name, and trial info (Admin only)</returns>
    [Get("/api/v1/settings")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<SettingsResponse>>> GetSettingsAsync();

    /// <summary>
    /// PUT /api/v1/settings
    /// Update application settings (Admin only)
    /// Updates general settings, appearance, and trial configuration
    /// Returns wrapped in ApiResponse<SettingsResponse>
    /// </summary>
    /// <param name="request">Settings update request with shop name, theme, license, etc.</param>
    /// <returns>Updated settings</returns>
    [Put("/api/v1/settings")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<SettingsResponse>>> UpdateSettingsAsync([Body] UpdateSettingsRequest request);

    /// <summary>
    /// PUT /api/v1/settings/appearance
    /// Update appearance settings (SalesAgent and User roles only)
    /// Allows non-admin users to update their theme preference
    /// Returns wrapped in ApiResponse<SettingsResponse>
    /// </summary>
    /// <param name="request">Appearance update request (theme only)</param>
    /// <returns>Updated settings</returns>
    [Put("/api/v1/settings/appearance")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<SettingsResponse>>> UpdateAppearanceAsync([Body] UpdateAppearanceRequest request);
}
