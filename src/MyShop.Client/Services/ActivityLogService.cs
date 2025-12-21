// ============================================================================
// ACTIVITY LOG SERVICE
// File: Services/ActivityLogService.cs
// Description: Tracks and logs user activities for audit trail
// ============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.Client.Services;

/// <summary>
/// Service for tracking and logging user activities throughout the application.
/// Provides audit trail capabilities for debugging, compliance, and analytics.
/// </summary>
public class ActivityLogService : IActivityLogService
{
    #region Fields

    private readonly ObservableCollection<ActivityLogEntry> _recentActivities;
    private readonly List<ActivityLogEntry> _allActivities;
    private string _logFilePath;
    private readonly int _maxRecentItems;
    private readonly object _lock = new();
    private bool _isEnabled;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public ActivityLogService(int maxRecentItems = 100)
    {
        _maxRecentItems = maxRecentItems;
        _recentActivities = new ObservableCollection<ActivityLogEntry>();
        _allActivities = new List<ActivityLogEntry>();
        _isEnabled = true;
        _isInitialized = false;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets recent activities for display in UI
    /// </summary>
    public ObservableCollection<ActivityLogEntry> RecentActivities
    {
        get
        {
            EnsureInitialized();
            return _recentActivities;
        }
    }

    /// <summary>
    /// Gets or sets whether activity logging is enabled
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Ensures the service is initialized (lazy initialization)
    /// </summary>
    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        try
        {
            // Set log file path in local app data
            var localFolder = ApplicationData.Current.LocalFolder.Path;
            _logFilePath = Path.Combine(localFolder, "ActivityLogs");
            
            // Ensure directory exists
            Directory.CreateDirectory(_logFilePath);
            
            _isInitialized = true;
            
            // Load today's activities
            _ = LoadTodaysActivitiesAsync();
        }
        catch (Exception ex)
        {
            // Fallback to temp directory if ApplicationData is not available
            _logFilePath = Path.Combine(Path.GetTempPath(), "MyShop", "ActivityLogs");
            Directory.CreateDirectory(_logFilePath);
            _isInitialized = true;
            System.Diagnostics.Debug.WriteLine($"ActivityLogService: Failed to use ApplicationData, using temp folder: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a user activity
    /// </summary>
    public async Task LogActivityAsync(
        ActivityType type,
        string action,
        string details = null,
        string entityType = null,
        string entityId = null,
        Dictionary<string, object> metadata = null)
    {
        if (!_isEnabled) return;

        EnsureInitialized();

        var entry = new ActivityLogEntry
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTime.Now,
            Type = type,
            Action = action,
            Details = details,
            EntityType = entityType,
            EntityId = entityId,
            Metadata = metadata,
            UserId = GetCurrentUserId(),
            UserName = GetCurrentUserName(),
            SessionId = GetSessionId()
        };

        lock (_lock)
        {
            _allActivities.Add(entry);
            
            // Keep recent activities limited
            while (_recentActivities.Count >= _maxRecentItems)
            {
                _recentActivities.RemoveAt(_recentActivities.Count - 1);
            }
            _recentActivities.Insert(0, entry);
        }

        // Save to file asynchronously
        await SaveActivityAsync(entry);
    }

    /// <summary>
    /// Logs a navigation event
    /// </summary>
    public Task LogNavigationAsync(string fromPage, string toPage)
    {
        return LogActivityAsync(
            ActivityType.Navigation,
            "Navigated",
            $"From '{fromPage}' to '{toPage}'",
            "Page",
            toPage,
            new Dictionary<string, object>
            {
                ["FromPage"] = fromPage,
                ["ToPage"] = toPage
            });
    }

    /// <summary>
    /// Logs a data creation event
    /// </summary>
    public Task LogCreateAsync(string entityType, string entityId, string description = null)
    {
        return LogActivityAsync(
            ActivityType.Create,
            $"Created {entityType}",
            description ?? $"Created new {entityType} with ID: {entityId}",
            entityType,
            entityId);
    }

    /// <summary>
    /// Logs a data update event
    /// </summary>
    public Task LogUpdateAsync(string entityType, string entityId, string description = null, Dictionary<string, object> changes = null)
    {
        return LogActivityAsync(
            ActivityType.Update,
            $"Updated {entityType}",
            description ?? $"Updated {entityType} with ID: {entityId}",
            entityType,
            entityId,
            changes);
    }

    /// <summary>
    /// Logs a data deletion event
    /// </summary>
    public Task LogDeleteAsync(string entityType, string entityId, string description = null)
    {
        return LogActivityAsync(
            ActivityType.Delete,
            $"Deleted {entityType}",
            description ?? $"Deleted {entityType} with ID: {entityId}",
            entityType,
            entityId);
    }

    /// <summary>
    /// Logs a search event
    /// </summary>
    public Task LogSearchAsync(string searchQuery, string searchContext, int resultsCount)
    {
        return LogActivityAsync(
            ActivityType.Search,
            "Performed Search",
            $"Searched for '{searchQuery}' in {searchContext}, found {resultsCount} results",
            "Search",
            null,
            new Dictionary<string, object>
            {
                ["Query"] = searchQuery,
                ["Context"] = searchContext,
                ["ResultsCount"] = resultsCount
            });
    }

    /// <summary>
    /// Logs an export event
    /// </summary>
    public Task LogExportAsync(string exportType, string format, int recordCount)
    {
        return LogActivityAsync(
            ActivityType.Export,
            $"Exported {exportType}",
            $"Exported {recordCount} {exportType} records as {format}",
            exportType,
            null,
            new Dictionary<string, object>
            {
                ["Format"] = format,
                ["RecordCount"] = recordCount
            });
    }

    /// <summary>
    /// Logs an import event
    /// </summary>
    public Task LogImportAsync(string importType, int recordCount, int successCount, int failedCount)
    {
        return LogActivityAsync(
            ActivityType.Import,
            $"Imported {importType}",
            $"Imported {recordCount} {importType} records: {successCount} success, {failedCount} failed",
            importType,
            null,
            new Dictionary<string, object>
            {
                ["RecordCount"] = recordCount,
                ["SuccessCount"] = successCount,
                ["FailedCount"] = failedCount
            });
    }

    /// <summary>
    /// Logs an error event
    /// </summary>
    public Task LogErrorAsync(string errorMessage, string source, Exception exception = null)
    {
        return LogActivityAsync(
            ActivityType.Error,
            "Error Occurred",
            errorMessage,
            "Error",
            null,
            new Dictionary<string, object>
            {
                ["Source"] = source,
                ["ExceptionType"] = exception?.GetType().Name,
                ["StackTrace"] = exception?.StackTrace?.Substring(0, Math.Min(500, exception.StackTrace?.Length ?? 0))
            });
    }

    /// <summary>
    /// Logs a login event
    /// </summary>
    public Task LogLoginAsync(string userId, string userName, bool success)
    {
        return LogActivityAsync(
            success ? ActivityType.Login : ActivityType.LoginFailed,
            success ? "Logged In" : "Login Failed",
            success ? $"User '{userName}' logged in successfully" : $"Failed login attempt for user '{userName}'",
            "User",
            userId);
    }

    /// <summary>
    /// Logs a logout event
    /// </summary>
    public Task LogLogoutAsync(string userId, string userName)
    {
        return LogActivityAsync(
            ActivityType.Logout,
            "Logged Out",
            $"User '{userName}' logged out",
            "User",
            userId);
    }

    /// <summary>
    /// Logs a settings change event
    /// </summary>
    public Task LogSettingsChangeAsync(string settingName, object oldValue, object newValue)
    {
        return LogActivityAsync(
            ActivityType.SettingsChange,
            "Changed Settings",
            $"Changed '{settingName}' from '{oldValue}' to '{newValue}'",
            "Settings",
            settingName,
            new Dictionary<string, object>
            {
                ["OldValue"] = oldValue,
                ["NewValue"] = newValue
            });
    }

    /// <summary>
    /// Gets activities within a date range
    /// </summary>
    public async Task<List<ActivityLogEntry>> GetActivitiesAsync(
        DateTime? from = null,
        DateTime? to = null,
        ActivityType? type = null,
        string entityType = null,
        int? limit = null)
    {
        var startDate = from ?? DateTime.Today.AddDays(-30);
        var endDate = to ?? DateTime.Now;
        
        var activities = new List<ActivityLogEntry>();

        // Load from files for the date range
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayActivities = await LoadActivitiesForDateAsync(date);
            activities.AddRange(dayActivities);
        }

        // Apply filters
        var filtered = activities.Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate);

        if (type.HasValue)
        {
            filtered = filtered.Where(a => a.Type == type.Value);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            filtered = filtered.Where(a => a.EntityType == entityType);
        }

        var result = filtered.OrderByDescending(a => a.Timestamp).ToList();

        if (limit.HasValue)
        {
            result = result.Take(limit.Value).ToList();
        }

        return result;
    }

    /// <summary>
    /// Gets activity statistics
    /// </summary>
    public async Task<ActivityStatistics> GetStatisticsAsync(DateTime? from = null, DateTime? to = null)
    {
        var activities = await GetActivitiesAsync(from, to);

        return new ActivityStatistics
        {
            TotalActivities = activities.Count,
            ByType = activities.GroupBy(a => a.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByEntityType = activities.Where(a => !string.IsNullOrEmpty(a.EntityType))
                .GroupBy(a => a.EntityType)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByDay = activities.GroupBy(a => a.Timestamp.Date)
                .ToDictionary(g => g.Key, g => g.Count()),
            MostActiveHours = activities.GroupBy(a => a.Timestamp.Hour)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList()
        };
    }

    /// <summary>
    /// Clears activities older than specified days
    /// </summary>
    public async Task ClearOldActivitiesAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.Today.AddDays(-daysToKeep);
        
        var files = Directory.GetFiles(_logFilePath, "activities_*.json");
        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (DateTime.TryParseExact(fileName.Replace("activities_", ""), "yyyy-MM-dd", 
                null, System.Globalization.DateTimeStyles.None, out var fileDate))
            {
                if (fileDate < cutoffDate)
                {
                    File.Delete(file);
                }
            }
        }

        lock (_lock)
        {
            _allActivities.RemoveAll(a => a.Timestamp < cutoffDate);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Exports activities to JSON file
    /// </summary>
    public async Task<string> ExportActivitiesAsync(DateTime? from = null, DateTime? to = null)
    {
        var activities = await GetActivitiesAsync(from, to);
        
        var exportPath = Path.Combine(_logFilePath, $"export_activities_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        
        var json = JsonSerializer.Serialize(activities, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        await File.WriteAllTextAsync(exportPath, json);
        
        return exportPath;
    }

    #endregion

    #region Private Methods

    private async Task SaveActivityAsync(ActivityLogEntry entry)
    {
        try
        {
            var fileName = $"activities_{entry.Timestamp:yyyy-MM-dd}.json";
            var filePath = Path.Combine(_logFilePath, fileName);

            var activities = new List<ActivityLogEntry>();
            
            if (File.Exists(filePath))
            {
                var existingJson = await File.ReadAllTextAsync(filePath);
                activities = JsonSerializer.Deserialize<List<ActivityLogEntry>>(existingJson) ?? new List<ActivityLogEntry>();
            }

            activities.Add(entry);

            var json = JsonSerializer.Serialize(activities, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save activity log: {ex.Message}");
        }
    }

    private async Task<List<ActivityLogEntry>> LoadActivitiesForDateAsync(DateTime date)
    {
        try
        {
            var fileName = $"activities_{date:yyyy-MM-dd}.json";
            var filePath = Path.Combine(_logFilePath, fileName);

            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<List<ActivityLogEntry>>(json) ?? new List<ActivityLogEntry>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load activity log: {ex.Message}");
        }

        return new List<ActivityLogEntry>();
    }

    private async Task LoadTodaysActivitiesAsync()
    {
        var todayActivities = await LoadActivitiesForDateAsync(DateTime.Today);
        
        lock (_lock)
        {
            _allActivities.AddRange(todayActivities);
            
            foreach (var activity in todayActivities.OrderByDescending(a => a.Timestamp).Take(_maxRecentItems))
            {
                _recentActivities.Add(activity);
            }
        }
    }

    private string GetCurrentUserId()
    {
        // TODO: Get from CurrentUserService when available
        return "current-user-id";
    }

    private string GetCurrentUserName()
    {
        // TODO: Get from CurrentUserService when available
        return "Current User";
    }

    private string GetSessionId()
    {
        // Generate session ID on app start, store in static field
        return _sessionId ??= Guid.NewGuid().ToString();
    }

    private static string _sessionId;

    #endregion
}

#region Interfaces

public interface IActivityLogService
{
    ObservableCollection<ActivityLogEntry> RecentActivities { get; }
    bool IsEnabled { get; set; }
    
    Task LogActivityAsync(ActivityType type, string action, string details = null, 
        string entityType = null, string entityId = null, Dictionary<string, object> metadata = null);
    
    Task LogNavigationAsync(string fromPage, string toPage);
    Task LogCreateAsync(string entityType, string entityId, string description = null);
    Task LogUpdateAsync(string entityType, string entityId, string description = null, Dictionary<string, object> changes = null);
    Task LogDeleteAsync(string entityType, string entityId, string description = null);
    Task LogSearchAsync(string searchQuery, string searchContext, int resultsCount);
    Task LogExportAsync(string exportType, string format, int recordCount);
    Task LogImportAsync(string importType, int recordCount, int successCount, int failedCount);
    Task LogErrorAsync(string errorMessage, string source, Exception exception = null);
    Task LogLoginAsync(string userId, string userName, bool success);
    Task LogLogoutAsync(string userId, string userName);
    Task LogSettingsChangeAsync(string settingName, object oldValue, object newValue);
    
    Task<List<ActivityLogEntry>> GetActivitiesAsync(DateTime? from = null, DateTime? to = null, 
        ActivityType? type = null, string entityType = null, int? limit = null);
    Task<ActivityStatistics> GetStatisticsAsync(DateTime? from = null, DateTime? to = null);
    Task ClearOldActivitiesAsync(int daysToKeep = 30);
    Task<string> ExportActivitiesAsync(DateTime? from = null, DateTime? to = null);
}

#endregion

#region Models

/// <summary>
/// Represents a single activity log entry
/// </summary>
public class ActivityLogEntry
{
    public string Id { get; set; }
    public DateTime Timestamp { get; set; }
    public ActivityType Type { get; set; }
    public string Action { get; set; }
    public string Details { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string SessionId { get; set; }
    public Dictionary<string, object> Metadata { get; set; }

    /// <summary>
    /// Gets icon for the activity type
    /// </summary>
    public string Icon => Type switch
    {
        ActivityType.Navigation => "\uE8A5",    // Navigation icon
        ActivityType.Create => "\uE710",        // Add icon
        ActivityType.Update => "\uE70F",        // Edit icon
        ActivityType.Delete => "\uE74D",        // Delete icon
        ActivityType.Search => "\uE721",        // Search icon
        ActivityType.Export => "\uE898",        // Download icon
        ActivityType.Import => "\uE896",        // Upload icon
        ActivityType.Login => "\uE77B",         // Contact icon
        ActivityType.Logout => "\uE7E8",        // Sign out icon
        ActivityType.LoginFailed => "\uE7BA",   // Warning icon
        ActivityType.Error => "\uE783",         // Error icon
        ActivityType.SettingsChange => "\uE713", // Settings icon
        ActivityType.View => "\uE7B3",          // View icon
        ActivityType.Print => "\uE749",         // Print icon
        _ => "\uE946"                           // Info icon
    };

    /// <summary>
    /// Gets display text for time ago
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.Now - Timestamp;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return Timestamp.ToString("MMM dd, yyyy");
        }
    }
}

/// <summary>
/// Types of activities that can be logged
/// </summary>
public enum ActivityType
{
    Navigation,
    Create,
    Update,
    Delete,
    Search,
    Export,
    Import,
    Login,
    Logout,
    LoginFailed,
    Error,
    SettingsChange,
    View,
    Print,
    Other
}

/// <summary>
/// Activity statistics for reporting
/// </summary>
public class ActivityStatistics
{
    public int TotalActivities { get; set; }
    public Dictionary<ActivityType, int> ByType { get; set; }
    public Dictionary<string, int> ByEntityType { get; set; }
    public Dictionary<DateTime, int> ByDay { get; set; }
    public List<int> MostActiveHours { get; set; }
}

#endregion
