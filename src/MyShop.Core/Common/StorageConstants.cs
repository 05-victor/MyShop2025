using System;
using System.IO;

namespace MyShop.Core.Common;

/// <summary>
/// Centralized storage paths for the application.
/// All file storage locations are defined here to avoid "lost files" scattered around.
/// 
/// Directory Structure:
/// AppData/Local/MyShop2025/
/// ├── app.config.json              # App config (runtime overrides)
/// ├── logs/                         # App logs (production mode)
/// │   ├── app-2025-11-28.log
/// │   └── errors-2025-11-28.log
/// ├── cache/                        # Global cache (images, offline data)
/// └── users/
///     └── {UserId}/
///         ├── credentials.enc       # Token (DPAPI encrypted)
///         ├── preferences.json      # User settings (theme, layout)
///         ├── avatar/               # User avatar images
///         ├── exports/              # User exports (Excel, CSV)
///         └── cache/                # Per-user cache
/// </summary>
public static class StorageConstants
{
    /// <summary>
    /// Application name used for folder naming
    /// </summary>
    public const string AppName = "MyShop2025";

    /// <summary>
    /// Currently logged-in user ID. Set after login, cleared on logout.
    /// Used by services to get user-specific paths.
    /// </summary>
    public static string? CurrentUserId { get; private set; }

    /// <summary>
    /// Set the current user ID (call after login)
    /// </summary>
    public static void SetCurrentUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        CurrentUserId = userId;
        EnsureUserDirectoriesExist(userId);
        System.Diagnostics.Debug.WriteLine($"[StorageConstants] CurrentUserId set: {userId}");
    }

    /// <summary>
    /// Clear the current user ID (call on logout)
    /// </summary>
    public static void ClearCurrentUser()
    {
        System.Diagnostics.Debug.WriteLine($"[StorageConstants] CurrentUserId cleared: {CurrentUserId}");
        CurrentUserId = null;
    }

    /// <summary>
    /// Root folder in LocalApplicationData
    /// e.g., C:\Users\{User}\AppData\Local\MyShop2025
    /// </summary>
    public static string AppDataRoot =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppName);

    /// <summary>
    /// Global logs directory (production mode)
    /// </summary>
    public static string LogsDirectory =>
        Path.Combine(AppDataRoot, "logs");

    /// <summary>
    /// Global cache directory
    /// </summary>
    public static string CacheDirectory =>
        Path.Combine(AppDataRoot, "cache");

    /// <summary>
    /// Users data root directory
    /// </summary>
    public static string UsersDirectory =>
        Path.Combine(AppDataRoot, "users");

    /// <summary>
    /// Session directory for temporary/anonymous data
    /// Used before user is authenticated
    /// </summary>
    public static string SessionDirectory =>
        Path.Combine(AppDataRoot, "session");

    /// <summary>
    /// Temporary credentials file (before login completes)
    /// </summary>
    public static string TempCredentialsFile =>
        Path.Combine(SessionDirectory, "temp_credentials.enc");

    /// <summary>
    /// File storing the last logged-in user ID for Remember Me feature
    /// </summary>
    public static string LastLoggedInUserFile =>
        Path.Combine(AppDataRoot, "last_user.txt");

    /// <summary>
    /// Save the last logged-in user ID (for Remember Me auto-login)
    /// </summary>
    public static void SaveLastLoggedInUser(string userId)
    {
        try
        {
            EnsureDirectoryExists(AppDataRoot);
            File.WriteAllText(LastLoggedInUserFile, userId);
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] Saved last logged-in user: {userId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] Failed to save last user: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the last logged-in user ID (for Remember Me auto-login)
    /// </summary>
    public static string? GetLastLoggedInUser()
    {
        try
        {
            if (File.Exists(LastLoggedInUserFile))
            {
                var userId = File.ReadAllText(LastLoggedInUserFile).Trim();
                if (!string.IsNullOrEmpty(userId))
                {
                    System.Diagnostics.Debug.WriteLine($"[StorageConstants] Found last logged-in user: {userId}");
                    return userId;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] Failed to read last user: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Clear the last logged-in user (on explicit logout)
    /// </summary>
    public static void ClearLastLoggedInUser()
    {
        try
        {
            if (File.Exists(LastLoggedInUserFile))
            {
                File.Delete(LastLoggedInUserFile);
                System.Diagnostics.Debug.WriteLine("[StorageConstants] Cleared last logged-in user");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] Failed to clear last user: {ex.Message}");
        }
    }

    #region Per-User Paths

    /// <summary>
    /// Get user-specific directory
    /// </summary>
    public static string GetUserDirectory(string userId) =>
        Path.Combine(UsersDirectory, SanitizeUserId(userId));

    /// <summary>
    /// Get user credentials file path (DPAPI encrypted)
    /// </summary>
    public static string GetUserCredentialsFile(string userId) =>
        Path.Combine(GetUserDirectory(userId), "credentials.enc");

    /// <summary>
    /// Get user preferences file path
    /// </summary>
    public static string GetUserPreferencesFile(string userId) =>
        Path.Combine(GetUserDirectory(userId), "preferences.json");

    /// <summary>
    /// Get user avatar directory
    /// </summary>
    public static string GetUserAvatarDirectory(string userId) =>
        Path.Combine(GetUserDirectory(userId), "avatar");

    /// <summary>
    /// Get user exports directory
    /// </summary>
    public static string GetUserExportsDirectory(string userId) =>
        Path.Combine(GetUserDirectory(userId), "exports");

    /// <summary>
    /// Get user cache directory
    /// </summary>
    public static string GetUserCacheDirectory(string userId) =>
        Path.Combine(GetUserDirectory(userId), "cache");

    /// <summary>
    /// Get the full path for an export file.
    /// Uses user exports directory if user is logged in, otherwise temp directory.
    /// </summary>
    /// <param name="fileName">The file name (e.g., "Products_Export_20251128.csv")</param>
    /// <returns>Full path to the export file</returns>
    public static string GetExportFilePath(string fileName)
    {
        string exportDirectory;
        
        if (!string.IsNullOrEmpty(CurrentUserId))
        {
            exportDirectory = GetUserExportsDirectory(CurrentUserId);
            EnsureDirectoryExists(exportDirectory);
        }
        else
        {
            exportDirectory = Path.GetTempPath();
        }
        
        return Path.Combine(exportDirectory, fileName);
    }

    /// <summary>
    /// Open Windows Explorer and select the specified file.
    /// Call this after exporting a file to show the user where it was saved.
    /// </summary>
    /// <param name="filePath">Full path to the file to select in Explorer</param>
    public static void OpenExplorerAndSelectFile(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                // If file doesn't exist, try to open the directory
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", directory);
                }
                return;
            }

            // Open Explorer and select the file
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] Opened Explorer at: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] Failed to open Explorer: {ex.Message}");
        }
    }

    /// <summary>
    /// Open the user's exports folder in Windows Explorer.
    /// </summary>
    public static void OpenExportsFolder()
    {
        try
        {
            string exportDirectory;
            
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                exportDirectory = GetUserExportsDirectory(CurrentUserId);
                EnsureDirectoryExists(exportDirectory);
            }
            else
            {
                exportDirectory = Path.GetTempPath();
            }

            System.Diagnostics.Process.Start("explorer.exe", exportDirectory);
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] Opened exports folder: {exportDirectory}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] Failed to open exports folder: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get all user IDs that have stored credentials.
    /// Used for auto-login to check which users have saved tokens.
    /// </summary>
    /// <returns>List of user IDs with credentials files</returns>
    public static string[] GetAllStoredUserIds()
    {
        try
        {
            if (!Directory.Exists(UsersDirectory))
                return Array.Empty<string>();

            var userDirs = Directory.GetDirectories(UsersDirectory);
            var usersWithCredentials = new System.Collections.Generic.List<string>();

            foreach (var dir in userDirs)
            {
                var userId = Path.GetFileName(dir);
                var credentialsFile = GetUserCredentialsFile(userId);
                
                if (File.Exists(credentialsFile))
                {
                    usersWithCredentials.Add(userId);
                    System.Diagnostics.Debug.WriteLine($"[StorageConstants] Found credentials for user: {userId}");
                }
            }

            return usersWithCredentials.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StorageConstants] GetAllStoredUserIds failed: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Ensure a directory exists, creating it if necessary
    /// </summary>
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// Ensure all base directories exist
    /// </summary>
    public static void EnsureBaseDirectoriesExist()
    {
        EnsureDirectoryExists(AppDataRoot);
        EnsureDirectoryExists(LogsDirectory);
        EnsureDirectoryExists(CacheDirectory);
        EnsureDirectoryExists(UsersDirectory);
        EnsureDirectoryExists(SessionDirectory);
    }

    /// <summary>
    /// Ensure user directories exist
    /// </summary>
    public static void EnsureUserDirectoriesExist(string userId)
    {
        EnsureDirectoryExists(GetUserDirectory(userId));
        EnsureDirectoryExists(GetUserAvatarDirectory(userId));
        EnsureDirectoryExists(GetUserExportsDirectory(userId));
        EnsureDirectoryExists(GetUserCacheDirectory(userId));
    }

    /// <summary>
    /// Sanitize user ID for use in file paths
    /// Removes invalid characters and limits length
    /// </summary>
    private static string SanitizeUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "anonymous";

        // Remove invalid path characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", userId.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Limit length to avoid path too long errors
        if (sanitized.Length > 50)
            sanitized = sanitized.Substring(0, 50);

        return sanitized;
    }

    /// <summary>
    /// Clean up session directory (call after successful login migration)
    /// </summary>
    public static void CleanupSessionDirectory()
    {
        try
        {
            if (Directory.Exists(SessionDirectory))
            {
                Directory.Delete(SessionDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Clean up user directory (call on account deletion or cleanup)
    /// </summary>
    public static void CleanupUserDirectory(string userId)
    {
        try
        {
            var userDir = GetUserDirectory(userId);
            if (Directory.Exists(userDir))
            {
                Directory.Delete(userDir, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    #endregion

    #region Log Settings

    /// <summary>
    /// Configuration for log storage location
    /// </summary>
    public static class LogSettings
    {
        /// <summary>
        /// If true, logs are stored in project directory (for development)
        /// If false, logs are stored in AppData (for production)
        /// 
        /// Toggle this during development for easy access to logs.
        /// In Release builds, this should always be false.
        /// </summary>
#if DEBUG
        public static bool StoreInProjectDirectory { get; set; } = true;
#else
        public static bool StoreInProjectDirectory { get; set; } = false;
#endif

        /// <summary>
        /// Project-relative log directory (only used when StoreInProjectDirectory = true)
        /// This is set by the application during startup based on assembly location
        /// </summary>
        public static string? ProjectLogDirectory { get; set; }

        /// <summary>
        /// Get the effective log directory based on settings
        /// </summary>
        public static string GetEffectiveLogDirectory()
        {
            if (StoreInProjectDirectory && !string.IsNullOrEmpty(ProjectLogDirectory))
            {
                return ProjectLogDirectory;
            }
            return LogsDirectory;
        }
    }

    #endregion
}
