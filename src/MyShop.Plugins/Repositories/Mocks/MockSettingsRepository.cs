using System.Text.Json;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for Settings using JSON data
/// </summary>
public class MockSettingsRepository
{
    private readonly string _jsonFilePath;
    private AppSettings? _appSettings;
    private SystemSettings? _systemSettings;
    private BusinessSettings? _businessSettings;

    public MockSettingsRepository()
    {
        _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "settings.json");
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] JSON file not found: {_jsonFilePath}");
                return;
            }

            var json = File.ReadAllText(_jsonFilePath);
            var jsonDoc = JsonDocument.Parse(json);

            // Load App Settings
            if (jsonDoc.RootElement.TryGetProperty("appSettings", out var appElement))
            {
                _appSettings = new AppSettings
                {
                    UserId = Guid.Parse(appElement.GetProperty("userId").GetString()!),
                    PageSize = appElement.GetProperty("pageSize").GetInt32(),
                    LastOpenedPage = appElement.GetProperty("lastOpenedPage").GetString()!,
                    Theme = appElement.GetProperty("theme").GetString()!,
                    Language = appElement.GetProperty("language").GetString()!,
                    CreatedAt = DateTime.Parse(appElement.GetProperty("createdAt").GetString()!),
                    UpdatedAt = appElement.TryGetProperty("updatedAt", out var updated) && updated.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(updated.GetString()!)
                        : null
                };

                // Load notifications
                if (appElement.TryGetProperty("notifications", out var notifElement))
                {
                    _appSettings.Notifications = new NotificationSettings
                    {
                        EmailNotifications = notifElement.GetProperty("emailNotifications").GetBoolean(),
                        LowStockAlerts = notifElement.GetProperty("lowStockAlerts").GetBoolean(),
                        NewOrderAlerts = notifElement.GetProperty("newOrderAlerts").GetBoolean(),
                        LowStockThreshold = notifElement.GetProperty("lowStockThreshold").GetInt32()
                    };
                }

                // Load display
                if (appElement.TryGetProperty("display", out var displayElement))
                {
                    _appSettings.Display = new DisplaySettings
                    {
                        ShowProductImages = displayElement.GetProperty("showProductImages").GetBoolean(),
                        CompactMode = displayElement.GetProperty("compactMode").GetBoolean(),
                        ShowRevenueDashboard = displayElement.GetProperty("showRevenueDashboard").GetBoolean()
                    };
                }
            }

            // Load System Settings
            if (jsonDoc.RootElement.TryGetProperty("systemSettings", out var sysElement))
            {
                _systemSettings = new SystemSettings
                {
                    ApplicationName = sysElement.GetProperty("applicationName").GetString()!,
                    Version = sysElement.GetProperty("version").GetString()!,
                    DefaultCurrency = sysElement.GetProperty("defaultCurrency").GetString()!,
                    TaxRate = sysElement.GetProperty("taxRate").GetDouble(),
                    TrialPeriodDays = sysElement.GetProperty("trialPeriodDays").GetInt32()
                };

                // Load features
                if (sysElement.TryGetProperty("features", out var featElement))
                {
                    _systemSettings.Features = new FeatureFlags
                    {
                        GoogleLogin = featElement.GetProperty("googleLogin").GetBoolean(),
                        EmailVerification = featElement.GetProperty("emailVerification").GetBoolean(),
                        TrialActivation = featElement.GetProperty("trialActivation").GetBoolean(),
                        AdminCodeVerification = featElement.GetProperty("adminCodeVerification").GetBoolean(),
                        DatabaseBackup = featElement.GetProperty("databaseBackup").GetBoolean(),
                        ProductImport = featElement.GetProperty("productImport").GetBoolean(),
                        ProductExport = featElement.GetProperty("productExport").GetBoolean()
                    };
                }
            }

            // Load Business Settings
            if (jsonDoc.RootElement.TryGetProperty("businessSettings", out var bizElement))
            {
                _businessSettings = new BusinessSettings
                {
                    StoreName = bizElement.GetProperty("storeName").GetString()!,
                    StoreAddress = bizElement.GetProperty("storeAddress").GetString()!,
                    StorePhone = bizElement.GetProperty("storePhone").GetString()!,
                    StoreEmail = bizElement.GetProperty("storeEmail").GetString()!,
                    StoreWebsite = bizElement.GetProperty("storeWebsite").GetString(),
                    BusinessRegistrationNumber = bizElement.GetProperty("businessRegistrationNumber").GetString(),
                    TaxCode = bizElement.GetProperty("taxCode").GetString(),
                    BankName = bizElement.GetProperty("bankName").GetString(),
                    BankAccountNumber = bizElement.GetProperty("bankAccountNumber").GetString(),
                    BankAccountName = bizElement.GetProperty("bankAccountName").GetString()
                };
            }

            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] Loaded all settings from JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] Error loading settings: {ex.Message}");
        }
    }

    public async Task<AppSettings?> GetAppSettingsAsync(Guid userId)
    {
        await Task.Delay(200);
        System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetAppSettingsAsync for user: {userId}");
        return _appSettings;
    }

    public async Task<AppSettings?> UpdateAppSettingsAsync(AppSettings settings)
    {
        await Task.Delay(300);
        
        if (_appSettings != null)
        {
            _appSettings.PageSize = settings.PageSize;
            _appSettings.LastOpenedPage = settings.LastOpenedPage;
            _appSettings.Theme = settings.Theme;
            _appSettings.Language = settings.Language;
            _appSettings.UpdatedAt = DateTime.UtcNow;

            if (settings.Notifications != null)
                _appSettings.Notifications = settings.Notifications;
            
            if (settings.Display != null)
                _appSettings.Display = settings.Display;
        }

        System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] Updated app settings");
        return _appSettings;
    }

    public async Task<SystemSettings?> GetSystemSettingsAsync()
    {
        await Task.Delay(150);
        System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetSystemSettingsAsync");
        return _systemSettings;
    }

    public async Task<BusinessSettings?> GetBusinessSettingsAsync()
    {
        await Task.Delay(150);
        System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetBusinessSettingsAsync");
        return _businessSettings;
    }
}

#region Data Models

public class AppSettings
{
    public Guid UserId { get; set; }
    public int PageSize { get; set; }
    public string LastOpenedPage { get; set; } = "DASHBOARD";
    public string Theme { get; set; } = "LIGHT";
    public string Language { get; set; } = "vi";
    public NotificationSettings? Notifications { get; set; }
    public DisplaySettings? Display { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class NotificationSettings
{
    public bool EmailNotifications { get; set; }
    public bool LowStockAlerts { get; set; }
    public bool NewOrderAlerts { get; set; }
    public int LowStockThreshold { get; set; }
}

public class DisplaySettings
{
    public bool ShowProductImages { get; set; }
    public bool CompactMode { get; set; }
    public bool ShowRevenueDashboard { get; set; }
}

public class SystemSettings
{
    public string ApplicationName { get; set; } = "MyShop 2025";
    public string Version { get; set; } = "1.0.0";
    public string DefaultCurrency { get; set; } = "VND";
    public double TaxRate { get; set; }
    public int TrialPeriodDays { get; set; }
    public FeatureFlags? Features { get; set; }
}

public class FeatureFlags
{
    public bool GoogleLogin { get; set; }
    public bool EmailVerification { get; set; }
    public bool TrialActivation { get; set; }
    public bool AdminCodeVerification { get; set; }
    public bool DatabaseBackup { get; set; }
    public bool ProductImport { get; set; }
    public bool ProductExport { get; set; }
}

public class BusinessSettings
{
    public string StoreName { get; set; } = string.Empty;
    public string StoreAddress { get; set; } = string.Empty;
    public string StorePhone { get; set; } = string.Empty;
    public string StoreEmail { get; set; } = string.Empty;
    public string? StoreWebsite { get; set; }
    public string? BusinessRegistrationNumber { get; set; }
    public string? TaxCode { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
}

#endregion
