using MyShop.Core.Common;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for Settings - loads data from settings.json
/// Provides full CRUD operations for testing settings functionality
/// </summary>
public class MockSettingsRepository
{
    public async Task<AppSettings?> GetAppSettingsAsync(Guid userId)
    {
        try
        {
            var data = await MockSettingsData.GetAppSettingsAsync(userId);
            if (data == null) return null;

            var settings = new AppSettings
            {
                UserId = userId,
                PageSize = data.Pagination?.DefaultPageSize ?? Core.Common.PaginationConstants.DefaultPageSize,
                LastOpenedPage = data.LastOpenedPage,
                Theme = data.Theme.ToUpper(),
                Language = data.Language,
                CreatedAt = data.CreatedAt,
                UpdatedAt = data.UpdatedAt,
                Notifications = new NotificationSettings
                {
                    EmailNotifications = data.Notifications?.EmailNotifications ?? true,
                    LowStockAlerts = data.Notifications?.LowStockAlerts ?? true,
                    NewOrderAlerts = data.Notifications?.NewOrderAlerts ?? true,
                    LowStockThreshold = data.Notifications?.LowStockThreshold ?? 10
                },
                Display = new DisplaySettings
                {
                    ShowProductImages = data.Display?.ShowProductImages ?? true,
                    CompactMode = data.Display?.CompactMode ?? false,
                    ShowRevenueDashboard = data.Display?.ShowRevenueDashboard ?? true
                }
            };

            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetAppSettingsAsync for user: {userId}");
            return settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetAppSettingsAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<AppSettings?> UpdateAppSettingsAsync(AppSettings settings)
    {
        try
        {
            var data = new MockSettingsData.AppSettingsData
            {
                UserId = settings.UserId.ToString(),
                Pagination = new MockSettingsData.PaginationData
                {
                    DefaultPageSize = settings.PageSize,
                    ProductsPageSize = settings.PageSize,
                    OrdersPageSize = settings.PageSize,
                    CustomersPageSize = settings.PageSize,
                    UsersPageSize = settings.PageSize,
                    AgentRequestsPageSize = settings.PageSize,
                    CommissionsPageSize = settings.PageSize
                },
                LastOpenedPage = settings.LastOpenedPage,
                Theme = settings.Theme.ToLower(),
                Language = settings.Language,
                CreatedAt = settings.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                Notifications = new MockSettingsData.NotificationsData
                {
                    EmailNotifications = settings.Notifications?.EmailNotifications ?? true,
                    LowStockAlerts = settings.Notifications?.LowStockAlerts ?? true,
                    NewOrderAlerts = settings.Notifications?.NewOrderAlerts ?? true,
                    LowStockThreshold = settings.Notifications?.LowStockThreshold ?? 10
                },
                Display = new MockSettingsData.DisplayData
                {
                    ShowProductImages = settings.Display?.ShowProductImages ?? true,
                    CompactMode = settings.Display?.CompactMode ?? false,
                    ShowRevenueDashboard = settings.Display?.ShowRevenueDashboard ?? true
                }
            };

            var result = await MockSettingsData.UpdateAppSettingsAsync(data);
            if (result == null) return null;

            settings.UpdatedAt = DateTime.UtcNow;
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] Updated app settings");
            return settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] UpdateAppSettingsAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<SystemSettings?> GetSystemSettingsAsync()
    {
        try
        {
            var data = await MockSettingsData.GetSystemSettingsAsync();
            if (data == null) return null;

            var settings = new SystemSettings
            {
                ApplicationName = data.ApplicationName,
                Version = data.Version,
                DefaultCurrency = data.DefaultCurrency,
                TaxRate = data.TaxRate * 100, // Convert from decimal to percentage
                TrialPeriodDays = data.TrialPeriodDays,
                TrialStartDate = data.TrialStartDate,
                UpgradeProUrl = data.UpgradeProUrl,
                SupportUrl = data.SupportUrl,
                Features = new FeatureFlags
                {
                    GoogleLogin = data.Features?.GoogleLogin ?? false,
                    EmailVerification = data.Features?.EmailVerification ?? true,
                    TrialActivation = data.Features?.TrialActivation ?? true,
                    AdminCodeVerification = data.Features?.AdminCodeVerification ?? true,
                    DatabaseBackup = data.Features?.DatabaseBackup ?? false,
                    ProductImport = data.Features?.ProductImport ?? false,
                    ProductExport = data.Features?.ProductExport ?? false
                }
            };

            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetSystemSettingsAsync - UpgradeUrl: {settings.UpgradeProUrl}");
            return settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetSystemSettingsAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<SystemSettings?> UpdateSystemSettingsAsync(SystemSettings settings)
    {
        try
        {
            var data = new MockSettingsData.SystemSettingsData
            {
                ApplicationName = settings.ApplicationName,
                Version = settings.Version,
                DefaultCurrency = settings.DefaultCurrency,
                TaxRate = settings.TaxRate / 100, // Convert from percentage to decimal
                TrialPeriodDays = settings.TrialPeriodDays,
                TrialStartDate = settings.TrialStartDate,
                UpgradeProUrl = settings.UpgradeProUrl,
                SupportUrl = settings.SupportUrl,
                Features = new MockSettingsData.FeatureFlagsData
                {
                    GoogleLogin = settings.Features?.GoogleLogin ?? false,
                    EmailVerification = settings.Features?.EmailVerification ?? true,
                    TrialActivation = settings.Features?.TrialActivation ?? true,
                    AdminCodeVerification = settings.Features?.AdminCodeVerification ?? true,
                    DatabaseBackup = settings.Features?.DatabaseBackup ?? false,
                    ProductImport = settings.Features?.ProductImport ?? false,
                    ProductExport = settings.Features?.ProductExport ?? false
                }
            };

            var result = await MockSettingsData.UpdateSystemSettingsAsync(data);
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] Updated system settings");
            return settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] UpdateSystemSettingsAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<BusinessSettings?> GetBusinessSettingsAsync()
    {
        try
        {
            var data = await MockSettingsData.GetBusinessSettingsAsync();
            if (data == null) return null;

            var settings = new BusinessSettings
            {
                StoreName = data.StoreName,
                StoreAddress = data.StoreAddress,
                StorePhone = data.StorePhone,
                StoreEmail = data.StoreEmail,
                StoreWebsite = data.StoreWebsite,
                BusinessRegistrationNumber = data.BusinessRegistrationNumber,
                TaxCode = data.TaxCode,
                BankName = data.BankName,
                BankAccountNumber = data.BankAccountNumber,
                BankAccountName = data.BankAccountName
            };

            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetBusinessSettingsAsync");
            return settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetBusinessSettingsAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<BusinessSettings?> UpdateBusinessSettingsAsync(BusinessSettings settings)
    {
        try
        {
            var data = new MockSettingsData.BusinessSettingsData
            {
                StoreName = settings.StoreName,
                StoreAddress = settings.StoreAddress,
                StorePhone = settings.StorePhone,
                StoreEmail = settings.StoreEmail,
                StoreWebsite = settings.StoreWebsite,
                BusinessRegistrationNumber = settings.BusinessRegistrationNumber,
                TaxCode = settings.TaxCode,
                BankName = settings.BankName,
                BankAccountNumber = settings.BankAccountNumber,
                BankAccountName = settings.BankAccountName
            };

            var result = await MockSettingsData.UpdateBusinessSettingsAsync(data);
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] Updated business settings");
            return settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] UpdateBusinessSettingsAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Activate a trial code to extend trial period
    /// Uses MockSystemActivationData for unified activation logic
    /// </summary>
    public async Task<Result<int>> ActivateTrialCodeAsync(string trialCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(trialCode))
            {
                return Result<int>.Failure("Trial code cannot be empty.");
            }

            // Use new unified MockSystemActivationData
            var validCode = await MockSystemActivationData.ValidateCodeAsync(trialCode);
            
            if (validCode != null && validCode.DurationDays.HasValue && validCode.DurationDays.Value > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] Trial code validated: +{validCode.DurationDays.Value} days");
                return Result<int>.Success(validCode.DurationDays.Value);
            }

            return Result<int>.Failure("Invalid trial code.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] ActivateTrialCodeAsync error: {ex.Message}");
            return Result<int>.Failure($"Failed to activate trial code: {ex.Message}");
        }
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
    public DateTime? TrialStartDate { get; set; }
    public string UpgradeProUrl { get; set; } = "https://facebook.com";
    public string SupportUrl { get; set; } = "https://facebook.com/myshop.support";
    public FeatureFlags? Features { get; set; }
    
    /// <summary>
    /// Calculate remaining trial days based on TrialStartDate and TrialPeriodDays
    /// </summary>
    public int RemainingTrialDays
    {
        get
        {
            if (TrialStartDate == null) return TrialPeriodDays;
            var elapsed = (DateTime.UtcNow - TrialStartDate.Value).Days;
            return Math.Max(0, TrialPeriodDays - elapsed);
        }
    }
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
