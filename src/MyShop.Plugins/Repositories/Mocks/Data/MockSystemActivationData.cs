using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for system activation - manages admin codes and licenses
/// Consolidates admin-codes.json and activation-codes.json logic
/// </summary>
public static class MockSystemActivationData
{
    private static List<AdminCodeData>? _adminCodes;
    private static LicenseData? _currentLicense;
    private static ActivationSettings? _settings;
    private static readonly object _lock = new object();
    
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Mocks",
        "Data",
        "Json",
        "system-activation.json"
    );

    private static readonly string _usersJsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Mocks",
        "Data",
        "Json",
        "users.json"
    );

    #region Data Loading

    private static void EnsureDataLoaded()
    {
        if (_adminCodes != null) return;

        lock (_lock)
        {
            if (_adminCodes != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<SystemActivationContainer>(jsonString, options);

                if (data?.AdminCodes != null)
                {
                    _adminCodes = data.AdminCodes;
                    _currentLicense = data.CurrentLicense;
                    _settings = data.Settings ?? new ActivationSettings();
                    System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Loaded {_adminCodes.Count} admin codes from JSON");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Error loading JSON: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _adminCodes = new List<AdminCodeData>();
        _currentLicense = null;
        _settings = new ActivationSettings
        {
            TrialDurationDays = 14,
            WarningDaysBeforeExpiry = 1,
            AllowMultipleAdmins = false
        };
        System.Diagnostics.Debug.WriteLine("[MockSystemActivationData] Initialized with default data");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Check if any admin exists in the system
    /// </summary>
    public static async Task<bool> HasAnyAdminAsync()
    {
        try
        {
            if (!File.Exists(_usersJsonFilePath))
            {
                return false;
            }

            var jsonString = await File.ReadAllTextAsync(_usersJsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<UsersContainer>(jsonString, options);

            if (data?.Users == null || data.Users.Count == 0)
            {
                return false;
            }

            // Check if any user has ADMIN role
            var hasAdmin = data.Users.Any(u =>
                u.RoleNames != null &&
                u.RoleNames.Any(r => r.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)));

            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] HasAnyAdmin: {hasAdmin}");
            return hasAdmin;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] HasAnyAdminAsync error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Validate an activation code (check if valid and available)
    /// </summary>
    public static async Task<AdminCodeData?> ValidateCodeAsync(string code)
    {
        EnsureDataLoaded();
        await Task.CompletedTask;

        var cleanCode = code.Trim().ToUpperInvariant().Replace(" ", "");

        var adminCode = _adminCodes!.FirstOrDefault(c =>
            c.Code.Replace("-", "").ToUpperInvariant() == cleanCode.Replace("-", "") &&
            c.Status.Equals("available", StringComparison.OrdinalIgnoreCase));

        if (adminCode != null)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Code validated: {code} -> Type: {adminCode.Type}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Code invalid or already used: {code}");
        }

        return adminCode;
    }

    /// <summary>
    /// Activate an admin code for a user
    /// </summary>
    public static async Task<(bool Success, string? ErrorMessage, LicenseData? License)> ActivateCodeAsync(string code, string userId)
    {
        EnsureDataLoaded();

        // Check if already has admin (single admin mode)
        // But allow current admin to upgrade their license (e.g., Trial -> Permanent)
        if (!_settings!.AllowMultipleAdmins && await HasAnyAdminAsync())
        {
            // Check if the current user is already the admin
            var isCurrentUserAdmin = _currentLicense != null && _currentLicense.UserId == userId;
            
            if (!isCurrentUserAdmin)
            {
                // Check in users.json if user is admin
                isCurrentUserAdmin = await IsUserAdminAsync(userId);
            }
            
            if (!isCurrentUserAdmin)
            {
                return (false, "System already has an administrator.", null);
            }
            
            // Current admin is upgrading their license - this is allowed
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Admin {userId} is upgrading their license");
        }

        // Validate code
        var adminCode = await ValidateCodeAsync(code);
        if (adminCode == null)
        {
            return (false, "Invalid or already used activation code.", null);
        }

        // Mark code as used
        adminCode.Status = "used";
        adminCode.UsedBy = userId;
        adminCode.UsedAt = DateTime.UtcNow;

        // Create license based on code type
        var license = new LicenseData
        {
            UserId = userId,
            Type = adminCode.Type,
            ActivatedAt = DateTime.UtcNow,
            CodeUsed = adminCode.Code
        };

        if (adminCode.Type.Equals("trial", StringComparison.OrdinalIgnoreCase))
        {
            var trialDays = adminCode.DurationDays ?? _settings.TrialDurationDays;
            license.ExpiresAt = DateTime.UtcNow.AddDays(trialDays);
            license.DurationDays = trialDays;
        }
        else
        {
            // Permanent license - no expiry
            license.ExpiresAt = null;
            license.DurationDays = null;
        }

        _currentLicense = license;

        // Update user role to ADMIN in users.json
        System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] About to update user role and save JSON...");
        await UpdateUserRoleToAdminAsync(userId, license);

        // Save changes to system-activation.json
        System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Saving activation data to JSON...");
        System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Code {adminCode.Code} status: {adminCode.Status}");
        await SaveDataToJsonAsync();

        System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Activated code: {code} for user: {userId}, Type: {license.Type}");
        return (true, null, license);
    }

    /// <summary>
    /// Get current license information
    /// </summary>
    public static async Task<LicenseData?> GetCurrentLicenseAsync()
    {
        EnsureDataLoaded();
        await Task.CompletedTask;
        return _currentLicense;
    }

    /// <summary>
    /// Get remaining trial days (0 if expired or permanent)
    /// </summary>
    public static async Task<int> GetRemainingTrialDaysAsync()
    {
        var license = await GetCurrentLicenseAsync();

        if (license == null)
        {
            return 0;
        }

        if (license.Type.Equals("permanent", StringComparison.OrdinalIgnoreCase))
        {
            return -1; // Indicates permanent license
        }

        if (license.ExpiresAt == null)
        {
            return 0;
        }

        var remaining = (license.ExpiresAt.Value - DateTime.UtcNow).Days;
        return Math.Max(0, remaining);
    }

    /// <summary>
    /// Check if trial is about to expire (within warning period)
    /// </summary>
    public static async Task<bool> IsTrialExpiringAsync()
    {
        EnsureDataLoaded();
        var remaining = await GetRemainingTrialDaysAsync();

        if (remaining < 0) return false; // Permanent license

        return remaining <= _settings!.WarningDaysBeforeExpiry && remaining > 0;
    }

    /// <summary>
    /// Check if trial has expired
    /// </summary>
    public static async Task<bool> IsTrialExpiredAsync()
    {
        var license = await GetCurrentLicenseAsync();

        if (license == null) return false;
        if (license.Type.Equals("permanent", StringComparison.OrdinalIgnoreCase)) return false;
        if (license.ExpiresAt == null) return false;

        return license.ExpiresAt.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Demote admin to customer when trial expires
    /// </summary>
    public static async Task<bool> DemoteAdminToCustomerAsync()
    {
        var license = await GetCurrentLicenseAsync();
        if (license == null) return false;

        try
        {
            // Update user role in users.json
            if (!File.Exists(_usersJsonFilePath)) return false;

            var jsonString = await File.ReadAllTextAsync(_usersJsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            var data = JsonSerializer.Deserialize<UsersContainer>(jsonString, options);
            if (data?.Users == null) return false;

            var user = data.Users.FirstOrDefault(u => u.Id == license.UserId);
            if (user == null) return false;

            // Change role from ADMIN to USER
            user.RoleNames = new List<string> { "USER" };
            user.IsTrialActive = false;
            user.TrialEndDate = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            // Save users.json
            var updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await File.WriteAllTextAsync(_usersJsonFilePath, updatedJson);

            // Clear current license
            _currentLicense = null;
            await SaveDataToJsonAsync();

            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Demoted user {license.UserId} from ADMIN to USER");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] DemoteAdminToCustomerAsync error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get activation settings
    /// </summary>
    public static ActivationSettings GetSettings()
    {
        EnsureDataLoaded();
        return _settings ?? new ActivationSettings();
    }

    #endregion

    #region Private Helpers

    private static async Task UpdateUserRoleToAdminAsync(string userId, LicenseData license)
    {
        try
        {
            if (!File.Exists(_usersJsonFilePath)) return;

            var jsonString = await File.ReadAllTextAsync(_usersJsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<UsersContainer>(jsonString, options);
            if (data?.Users == null) return;

            var user = data.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return;

            // Update role to ADMIN
            user.RoleNames = new List<string> { "ADMIN" };
            user.UpdatedAt = DateTime.UtcNow;

            // Set trial info if applicable
            if (license.Type.Equals("trial", StringComparison.OrdinalIgnoreCase))
            {
                user.IsTrialActive = true;
                user.TrialStartDate = license.ActivatedAt;
                user.TrialEndDate = license.ExpiresAt;
            }
            else
            {
                user.IsTrialActive = false;
                user.IsPermanentLicense = true;
                user.TrialStartDate = null;
                user.TrialEndDate = null;
            }

            // Save back to JSON
            var updatedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await File.WriteAllTextAsync(_usersJsonFilePath, updatedJson);

            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Updated user {userId} role to ADMIN");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] UpdateUserRoleToAdminAsync error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if a specific user is an admin
    /// </summary>
    private static async Task<bool> IsUserAdminAsync(string userId)
    {
        try
        {
            if (!File.Exists(_usersJsonFilePath))
            {
                return false;
            }

            var jsonString = await File.ReadAllTextAsync(_usersJsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<UsersContainer>(jsonString, options);
            if (data?.Users == null)
            {
                return false;
            }

            var user = data.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.RoleNames == null)
            {
                return false;
            }

            return user.RoleNames.Any(r => r.Equals("ADMIN", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] IsUserAdminAsync error: {ex.Message}");
            return false;
        }
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new SystemActivationContainer
            {
                AdminCodes = _adminCodes!,
                CurrentLicense = _currentLicense,
                Settings = _settings
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);

            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Saved to JSON at: {_jsonFilePath}");
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] CurrentLicense: {(_currentLicense != null ? $"Type={_currentLicense.Type}, UserId={_currentLicense.UserId}" : "null")}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockSystemActivationData] Error saving JSON: {ex.Message}");
        }
    }

    #endregion

    #region Data Classes

    private class SystemActivationContainer
    {
        public List<AdminCodeData> AdminCodes { get; set; } = new();
        public LicenseData? CurrentLicense { get; set; }
        public ActivationSettings? Settings { get; set; }
    }

    private class UsersContainer
    {
        public List<UserData> Users { get; set; } = new();
    }

    private class UserData
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> RoleNames { get; set; } = new();
        public bool IsTrialActive { get; set; }
        public bool IsPermanentLicense { get; set; }
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AdminCodeData
    {
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "trial"; // "trial" or "permanent"
        public int? DurationDays { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "available"; // "available" or "used"
        public string? UsedBy { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LicenseData
    {
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = "trial"; // "trial" or "permanent"
        public DateTime ActivatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? DurationDays { get; set; }
        public string CodeUsed { get; set; } = string.Empty;
    }

    public class ActivationSettings
    {
        public int TrialDurationDays { get; set; } = 14;
        public int WarningDaysBeforeExpiry { get; set; } = 1;
        public bool AllowMultipleAdmins { get; set; } = false;
    }

    #endregion
}
