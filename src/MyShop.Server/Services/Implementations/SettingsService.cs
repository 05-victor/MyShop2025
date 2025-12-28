using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using System.Reflection;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service implementation for managing application settings
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(
        ISettingsRepository settingsRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ILogger<SettingsService> logger)
    {
        _settingsRepository = settingsRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<SettingsResponse> GetSettingsAsync()
    {
        try
        {
            // Get global app settings
            var appSettings = await _settingsRepository.GetAppSettingsAsync();
            if (appSettings == null)
            {
                throw new InvalidOperationException("App settings not found in database");
            }

            // Get current user ID
            var currentUserId = _currentUserService.UserId;
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Get user preference (theme)
            var userPreference = await _settingsRepository.GetUserPreferenceAsync(currentUserId.Value);
            var theme = userPreference?.Theme ?? "Light";

            // Get current user to check role and trial info
            var currentUser = await _userRepository.GetByIdAsync(currentUserId.Value);
            if (currentUser == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Check if user is Admin
            var isAdmin = currentUser.Roles?.Any(r => r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase)) ?? false;

            // Build response
            var response = new SettingsResponse
            {
                // General Settings
                ShopName = appSettings.ShopName,
                Address = appSettings.Address,
                
                // Appearance
                Theme = theme,
                
                // App Info
                AppName = appSettings.AppName,
                Version = appSettings.Version,
                ReleaseDate = appSettings.ReleaseDate,
                License = appSettings.License,
                Support = appSettings.Support,
                
                // Trial info (only for Admin)
                IsTrialActive = isAdmin ? currentUser.IsTrialActive : false,
                TrialStartDate = isAdmin ? currentUser.TrialStartDate : null,
                TrialEndDate = isAdmin ? currentUser.TrialEndDate : null
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings");
            throw;
        }
    }

    public async Task<SettingsResponse> UpdateSettingsAsync(UpdateSettingsRequest request)
    {
        try
        {
            // Get current user ID
            var currentUserId = _currentUserService.UserId;
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Get current user to verify Admin role
            var currentUser = await _userRepository.GetByIdAsync(currentUserId.Value);
            if (currentUser == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var isAdmin = currentUser.Roles?.Any(r => r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("Only Admin can update settings");
            }

            // Update global app settings
            var appSettings = await _settingsRepository.GetAppSettingsAsync();
            if (appSettings == null)
            {
                throw new InvalidOperationException("App settings not found");
            }

            appSettings.ShopName = request.ShopName;
            appSettings.Address = request.Address;
            appSettings.AppName = request.AppName;
            appSettings.Version = request.Version;
            appSettings.ReleaseDate = request.ReleaseDate;
            appSettings.License = request.License;
            appSettings.Support = request.Support;
            appSettings.UpdatedBy = currentUserId.Value;

            await _settingsRepository.UpdateAppSettingsAsync(appSettings);

            // Update user preference (theme) for Admin
            var userPreference = new UserPreference
            {
                UserId = currentUserId.Value,
                Theme = request.Theme
            };
            await _settingsRepository.UpsertUserPreferenceAsync(userPreference);

            // Return updated settings
            return await GetSettingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            throw;
        }
    }

    public async Task<SettingsResponse> UpdateAppearanceAsync(UpdateAppearanceRequest request)
    {
        try
        {
            // Get current user ID
            var currentUserId = _currentUserService.UserId;
            if (currentUserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Get current user to verify NOT Admin
            var currentUser = await _userRepository.GetByIdAsync(currentUserId.Value);
            if (currentUser == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var isAdmin = currentUser.Roles?.Any(r => r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase)) ?? false;
            if (isAdmin)
            {
                throw new UnauthorizedAccessException("Admin should use main PUT endpoint instead");
            }

            // Update user preference (theme only)
            var userPreference = new UserPreference
            {
                UserId = currentUserId.Value,
                Theme = request.Theme
            };
            await _settingsRepository.UpsertUserPreferenceAsync(userPreference);

            // Return updated settings
            return await GetSettingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appearance");
            throw;
        }
    }
}
