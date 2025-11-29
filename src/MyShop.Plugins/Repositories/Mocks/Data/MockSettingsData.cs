using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for settings - loads from JSON file
/// Provides CRUD operations for testing settings functionality
/// </summary>
public static class MockSettingsData
{
    private static SettingsDataContainer? _data;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "settings.json");

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static void EnsureDataLoaded()
    {
        if (_data != null) return;

        lock (_lock)
        {
            if (_data != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[MockSettingsData] Settings JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                _data = JsonSerializer.Deserialize<SettingsDataContainer>(jsonString, _jsonOptions);

                if (_data == null)
                {
                    InitializeDefaultData();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MockSettingsData] Loaded settings from settings.json");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MockSettingsData] Error loading settings.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _data = new SettingsDataContainer
        {
            AppSettings = new AppSettingsData
            {
                UserId = "00000000-0000-0000-0000-000000000001",
                Pagination = new PaginationData
                {
                    DefaultPageSize = 10,
                    ProductsPageSize = 10,
                    OrdersPageSize = 10,
                    CustomersPageSize = 10,
                    UsersPageSize = 10,
                    AgentRequestsPageSize = 10,
                    CommissionsPageSize = 10
                },
                LastOpenedPage = "DASHBOARD",
                Theme = "light",
                Language = "en-US",
                Notifications = new NotificationsData
                {
                    EmailNotifications = true,
                    LowStockAlerts = true,
                    NewOrderAlerts = true,
                    LowStockThreshold = 10
                },
                Display = new DisplayData
                {
                    ShowProductImages = true,
                    CompactMode = false,
                    ShowRevenueDashboard = true
                },
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow
            },
            SystemSettings = new SystemSettingsData
            {
                ApplicationName = "MyShop 2025",
                Version = "1.0.0",
                DefaultCurrency = "VND",
                TaxRate = 0.1,
                TrialPeriodDays = 15,
                TrialStartDate = DateTime.UtcNow.AddDays(-5),
                UpgradeProUrl = "https://facebook.com",
                SupportUrl = "https://facebook.com/myshop.support",
                Features = new FeatureFlagsData
                {
                    GoogleLogin = false,
                    EmailVerification = true,
                    TrialActivation = true,
                    AdminCodeVerification = true,
                    DatabaseBackup = false,
                    ProductImport = false,
                    ProductExport = false
                }
            },
            BusinessSettings = new BusinessSettingsData
            {
                StoreName = "MyShop Electronics",
                StoreAddress = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                StorePhone = "028-3822-1234",
                StoreEmail = "contact@myshop.com",
                StoreWebsite = "https://myshop.com",
                BusinessRegistrationNumber = "0123456789",
                TaxCode = "0123456789-001",
                BankName = "Vietcombank",
                BankAccountNumber = "0123456789",
                BankAccountName = "CÔNG TY TNHH MYSHOP"
            }
        };

        System.Diagnostics.Debug.WriteLine("[MockSettingsData] Initialized with default data");
    }

    /// <summary>
    /// Save current settings data to JSON file
    /// </summary>
    public static async Task SaveToJsonAsync()
    {
        try
        {
            EnsureDataLoaded();
            var jsonString = JsonSerializer.Serialize(_data, _jsonOptions);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);
            System.Diagnostics.Debug.WriteLine("[MockSettingsData] Settings saved to JSON file");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSettingsData] Error saving settings.json: {ex.Message}");
        }
    }

    #region App Settings CRUD

    public static async Task<AppSettingsData?> GetAppSettingsAsync(Guid userId)
    {
        EnsureDataLoaded();
        await Task.Delay(100); // Simulate network delay

        // Return app settings (in a real scenario, this would be per-user)
        return _data?.AppSettings;
    }

    public static async Task<AppSettingsData?> UpdateAppSettingsAsync(AppSettingsData settings)
    {
        EnsureDataLoaded();
        await Task.Delay(200);

        if (_data == null) return null;

        _data.AppSettings = settings;
        _data.AppSettings.UpdatedAt = DateTime.UtcNow;
        await SaveToJsonAsync();

        System.Diagnostics.Debug.WriteLine("[MockSettingsData] App settings updated");
        return _data.AppSettings;
    }

    #endregion

    #region System Settings CRUD

    public static async Task<SystemSettingsData?> GetSystemSettingsAsync()
    {
        EnsureDataLoaded();
        await Task.Delay(100);

        return _data?.SystemSettings;
    }

    public static async Task<SystemSettingsData?> UpdateSystemSettingsAsync(SystemSettingsData settings)
    {
        EnsureDataLoaded();
        await Task.Delay(200);

        if (_data == null) return null;

        _data.SystemSettings = settings;
        await SaveToJsonAsync();

        System.Diagnostics.Debug.WriteLine("[MockSettingsData] System settings updated");
        return _data.SystemSettings;
    }

    #endregion

    #region Business Settings CRUD

    public static async Task<BusinessSettingsData?> GetBusinessSettingsAsync()
    {
        EnsureDataLoaded();
        await Task.Delay(100);

        return _data?.BusinessSettings;
    }

    public static async Task<BusinessSettingsData?> UpdateBusinessSettingsAsync(BusinessSettingsData settings)
    {
        EnsureDataLoaded();
        await Task.Delay(200);

        if (_data == null) return null;

        _data.BusinessSettings = settings;
        await SaveToJsonAsync();

        System.Diagnostics.Debug.WriteLine("[MockSettingsData] Business settings updated");
        return _data.BusinessSettings;
    }

    #endregion

    #region Data Models

    public class SettingsDataContainer
    {
        public AppSettingsData? AppSettings { get; set; }
        public SystemSettingsData? SystemSettings { get; set; }
        public BusinessSettingsData? BusinessSettings { get; set; }
    }

    public class AppSettingsData
    {
        public string? UserId { get; set; }
        public PaginationData? Pagination { get; set; }
        public string LastOpenedPage { get; set; } = "DASHBOARD";
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "en-US";
        public NotificationsData? Notifications { get; set; }
        public DisplayData? Display { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PaginationData
    {
        public int DefaultPageSize { get; set; } = 10;
        public int ProductsPageSize { get; set; } = 10;
        public int OrdersPageSize { get; set; } = 10;
        public int CustomersPageSize { get; set; } = 10;
        public int UsersPageSize { get; set; } = 10;
        public int AgentRequestsPageSize { get; set; } = 10;
        public int CommissionsPageSize { get; set; } = 10;
    }

    public class NotificationsData
    {
        public bool EmailNotifications { get; set; }
        public bool LowStockAlerts { get; set; }
        public bool NewOrderAlerts { get; set; }
        public int LowStockThreshold { get; set; }
    }

    public class DisplayData
    {
        public bool ShowProductImages { get; set; }
        public bool CompactMode { get; set; }
        public bool ShowRevenueDashboard { get; set; }
    }

    public class SystemSettingsData
    {
        public string ApplicationName { get; set; } = "MyShop 2025";
        public string Version { get; set; } = "1.0.0";
        public string DefaultCurrency { get; set; } = "VND";
        public double TaxRate { get; set; }
        public int TrialPeriodDays { get; set; } = 15;
        public DateTime? TrialStartDate { get; set; }
        public string UpgradeProUrl { get; set; } = "https://facebook.com";
        public string SupportUrl { get; set; } = "https://facebook.com/myshop.support";
        public FeatureFlagsData? Features { get; set; }

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

    public class FeatureFlagsData
    {
        public bool GoogleLogin { get; set; }
        public bool EmailVerification { get; set; }
        public bool TrialActivation { get; set; }
        public bool AdminCodeVerification { get; set; }
        public bool DatabaseBackup { get; set; }
        public bool ProductImport { get; set; }
        public bool ProductExport { get; set; }
    }

    public class BusinessSettingsData
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
}
