using MyShop.Core.Common;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Infrastructure;

/// <summary>
/// File-based implementation of ISettingsStorage
/// 
/// Storage Location:
/// - Per-user: AppData/Local/MyShop2025/users/{UserId}/preferences.json
/// - Anonymous: AppData/Local/MyShop2025/session/preferences.json
/// 
/// This ensures each user has their own preferences (theme, language, etc.)
/// </summary>
public class FileSettingsStorage : ISettingsStorage
{
    private string? _currentUserId;
    
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public FileSettingsStorage()
    {
        // Ensure base directories exist
        StorageConstants.EnsureBaseDirectoriesExist();
        System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Initialized, base path: {StorageConstants.AppDataRoot}");
    }

    /// <summary>
    /// Set the current user ID (call after login to enable per-user storage)
    /// </summary>
    public void SetCurrentUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var previousUserId = _currentUserId;
        _currentUserId = userId;

        System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] User set: {userId}");

        // Ensure user directories exist
        StorageConstants.EnsureUserDirectoriesExist(userId);

        // Migrate settings from session if they exist
        if (previousUserId == null)
        {
            MigrateFromSessionSettings();
        }
    }

    /// <summary>
    /// Clear the current user (call on logout)
    /// </summary>
    public void ClearCurrentUser()
    {
        System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] User cleared: {_currentUserId}");
        _currentUserId = null;
    }

    /// <summary>
    /// Get the current user ID
    /// </summary>
    public string? CurrentUserId => _currentUserId;

    public async Task<Result<AppSettings>> GetAsync()
    {
        try
        {
            var filePath = GetPreferencesFilePath();

            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Preferences file not found at: {filePath}, using defaults");
                return Result<AppSettings>.Success(new AppSettings());
            }

            await using var fs = File.OpenRead(filePath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(fs, _jsonOptions);
            
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Loaded preferences: Theme={settings?.Theme}, Language={settings?.Language}");
            
            return Result<AppSettings>.Success(settings ?? new AppSettings());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Error loading preferences: {ex.Message}");
            return Result<AppSettings>.Failure($"Failed to load settings: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> SaveAsync(AppSettings settings)
    {
        try
        {
            var filePath = GetPreferencesFilePath();
            var directory = Path.GetDirectoryName(filePath);
            
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Preferences saved to: {filePath}");
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Values: Theme={settings.Theme}, Language={settings.Language}");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Error saving preferences: {ex.Message}");
            return Result<Unit>.Failure($"Failed to save settings: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ResetAsync()
    {
        try
        {
            var filePath = GetPreferencesFilePath();
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Preferences file deleted: {filePath}");
            }
            
            await Task.CompletedTask;
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Error resetting preferences: {ex.Message}");
            return Result<Unit>.Failure($"Failed to reset settings: {ex.Message}");
        }
    }

    #region Private Methods

    /// <summary>
    /// Get the path to the preferences file
    /// Uses per-user path if user is set, otherwise session path
    /// </summary>
    private string GetPreferencesFilePath()
    {
        if (!string.IsNullOrEmpty(_currentUserId))
        {
            return StorageConstants.GetUserPreferencesFile(_currentUserId);
        }

        // No user set - use session location
        StorageConstants.EnsureDirectoryExists(StorageConstants.SessionDirectory);
        return Path.Combine(StorageConstants.SessionDirectory, "preferences.json");
    }

    /// <summary>
    /// Migrate settings from session location to user-specific location
    /// Called after user ID is set
    /// </summary>
    private void MigrateFromSessionSettings()
    {
        try
        {
            var sessionFile = Path.Combine(StorageConstants.SessionDirectory, "preferences.json");
            
            if (!File.Exists(sessionFile) || string.IsNullOrEmpty(_currentUserId))
                return;

            var userFile = StorageConstants.GetUserPreferencesFile(_currentUserId);
            var userDir = Path.GetDirectoryName(userFile);

            if (!string.IsNullOrEmpty(userDir))
            {
                Directory.CreateDirectory(userDir);
            }

            // Only migrate if user doesn't already have preferences
            if (!File.Exists(userFile))
            {
                File.Copy(sessionFile, userFile);
                System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Migrated session preferences to: {userFile}");
            }

            // Delete session file after successful migration
            File.Delete(sessionFile);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileSettingsStorage] Migration failed: {ex.Message}");
        }
    }

    #endregion
}
