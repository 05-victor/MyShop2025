namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for Settings - provides hardcoded mock data
/// Note: No MockSettingsData exists yet, using inline defaults
/// </summary>
public class MockSettingsRepository
{

    public async Task<AppSettings?> GetAppSettingsAsync(Guid userId)
    {
        try
        {
            await Task.Delay(200);
            
            var settings = new AppSettings
            {
                UserId = userId,
                PageSize = Core.Common.PaginationConstants.DefaultPageSize,
                LastOpenedPage = "DASHBOARD",
                Theme = "LIGHT",
                Language = "vi",
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                Notifications = new NotificationSettings
                {
                    EmailNotifications = true,
                    LowStockAlerts = true,
                    NewOrderAlerts = true,
                    LowStockThreshold = 10
                },
                Display = new DisplaySettings
                {
                    ShowProductImages = true,
                    CompactMode = false,
                    ShowRevenueDashboard = true
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
            await Task.Delay(300);
            
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
            await Task.Delay(150);
            
            var settings = new SystemSettings
            {
                ApplicationName = "MyShop 2025",
                Version = "1.0.0",
                DefaultCurrency = "VND",
                TaxRate = 10.0,
                TrialPeriodDays = 30,
                Features = new FeatureFlags
                {
                    GoogleLogin = true,
                    EmailVerification = false,
                    TrialActivation = true,
                    AdminCodeVerification = true,
                    DatabaseBackup = false,
                    ProductImport = true,
                    ProductExport = true
                }
            };
            
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetSystemSettingsAsync");
            return settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsRepository] GetSystemSettingsAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<BusinessSettings?> GetBusinessSettingsAsync()
    {
        try
        {
            await Task.Delay(150);
            
            var settings = new BusinessSettings
            {
                StoreName = "MyShop 2025",
                StoreAddress = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                StorePhone = "0901234567",
                StoreEmail = "contact@myshop2025.vn",
                StoreWebsite = "https://myshop2025.vn",
                BusinessRegistrationNumber = "0123456789",
                TaxCode = "0123456789-001",
                BankName = "Vietcombank",
                BankAccountNumber = "1234567890",
                BankAccountName = "CONG TY TNHH MYSHOP 2025"
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
