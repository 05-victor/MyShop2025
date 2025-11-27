using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for admin codes - loads from JSON file
/// </summary>
public static class MockAdminCodesData
{
    private static List<AdminCodeData>? _adminCodes;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "Mocks", 
        "Data", 
        "Json", 
        "admin-codes.json"
    );

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
                    System.Diagnostics.Debug.WriteLine($"Admin codes JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var data = JsonSerializer.Deserialize<AdminCodesContainer>(jsonString, options);
                
                if (data?.AdminCodes != null)
                {
                    _adminCodes = data.AdminCodes;
                    System.Diagnostics.Debug.WriteLine($"Loaded {_adminCodes.Count} admin codes from admin-codes.json");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading admin-codes.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        // Initialize empty list - data should be loaded from admin-codes.json
        _adminCodes = new List<AdminCodeData>();
        System.Diagnostics.Debug.WriteLine("[MockAdminCodesData] JSON file not found - initialized with empty admin codes list");
    }

    public static async Task<AdminCodeData?> ValidateCodeAsync(string code)
    {
        EnsureDataLoaded();
        await Task.Delay(100); // Simulate network delay

        var cleanCode = code.Replace("-", "").ToUpper();
        var adminCode = _adminCodes!.FirstOrDefault(c => 
            c.Code.Replace("-", "").ToUpper() == cleanCode && 
            c.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) && 
            (c.ExpiresAt == null || c.ExpiresAt > DateTime.UtcNow) &&
            (c.MaxUses < 0 || c.CurrentUses < c.MaxUses));

        return adminCode;
    }

    public static async Task<bool> MarkAsUsedAsync(string code, string userId)
    {
        EnsureDataLoaded();
        await Task.Delay(50);

        var cleanCode = code.Replace("-", "").ToUpper();
        var adminCode = _adminCodes!.FirstOrDefault(c => 
            c.Code.Replace("-", "").ToUpper() == cleanCode);

        if (adminCode == null) return false;

        adminCode.CurrentUses++;
        adminCode.UsedBy = userId;
        adminCode.UsedAt = DateTime.UtcNow;

        // If max uses reached, mark as Used
        if (adminCode.MaxUses > 0 && adminCode.CurrentUses >= adminCode.MaxUses)
        {
            adminCode.Status = "Used";
        }

        await SaveDataToJsonAsync();
        return true;
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new AdminCodesContainer
            {
                AdminCodes = _adminCodes!,
                Settings = new AdminCodeSettings
                {
                    RequireCodeForFirstUser = true,
                    RequireCodeForAdditionalAdmins = false,
                    AllowMultipleAdmins = true,
                    DefaultCodeExpiry = 365
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);
            
            System.Diagnostics.Debug.WriteLine("Successfully saved admin codes data to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving admin-codes.json: {ex.Message}");
        }
    }

    // Data container classes for JSON serialization/deserialization
    private class AdminCodesContainer
    {
        public List<AdminCodeData> AdminCodes { get; set; } = new();
        public AdminCodeSettings? Settings { get; set; }
    }

    private class AdminCodeSettings
    {
        public bool RequireCodeForFirstUser { get; set; }
        public bool RequireCodeForAdditionalAdmins { get; set; }
        public bool AllowMultipleAdmins { get; set; }
        public int DefaultCodeExpiry { get; set; }
    }

    public class AdminCodeData
    {
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Active"; // Active, Used, Expired
        public string? UsedBy { get; set; }
        public DateTime? UsedAt { get; set; }
        public int MaxUses { get; set; } = 1;
        public int CurrentUses { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
