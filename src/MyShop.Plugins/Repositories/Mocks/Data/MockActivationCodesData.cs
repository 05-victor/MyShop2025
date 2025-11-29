using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for activation/trial codes - loads from JSON file
/// </summary>
public static class MockActivationCodesData
{
    private static List<ActivationCodeData>? _activationCodes;
    private static List<CodeHistoryData>? _codeHistory;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "Mocks", 
        "Data", 
        "Json", 
        "activation-codes.json"
    );

    private static void EnsureDataLoaded()
    {
        if (_activationCodes != null) return;

        lock (_lock)
        {
            if (_activationCodes != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[MockActivationCodesData] JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var data = JsonSerializer.Deserialize<ActivationCodesContainer>(jsonString, options);
                
                if (data?.ActivationCodes != null)
                {
                    _activationCodes = data.ActivationCodes;
                    _codeHistory = data.CodeHistory ?? new List<CodeHistoryData>();
                    System.Diagnostics.Debug.WriteLine($"[MockActivationCodesData] Loaded {_activationCodes.Count} activation codes from JSON");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MockActivationCodesData] Error loading JSON: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _activationCodes = new List<ActivationCodeData>();
        _codeHistory = new List<CodeHistoryData>();
        System.Diagnostics.Debug.WriteLine("[MockActivationCodesData] Initialized with empty list");
    }

    /// <summary>
    /// Validate and activate a trial/license code
    /// </summary>
    public static async Task<(bool Success, int DaysToAdd, string? ErrorMessage)> ActivateCodeAsync(string code)
    {
        EnsureDataLoaded();
        await Task.Delay(300); // Simulate network delay

        var cleanCode = code.Trim().ToUpperInvariant();
        
        var activationCode = _activationCodes!.FirstOrDefault(c => 
            c.Code.Equals(cleanCode, StringComparison.OrdinalIgnoreCase));

        if (activationCode == null)
        {
            return (false, 0, "Invalid activation code. Please check and try again.");
        }

        // Check status
        if (activationCode.Status.Equals("Used", StringComparison.OrdinalIgnoreCase))
        {
            return (false, 0, "This code has already been used.");
        }

        if (activationCode.Status.Equals("Expired", StringComparison.OrdinalIgnoreCase))
        {
            return (false, 0, "This code has expired.");
        }

        // Check expiration date
        if (activationCode.ExpiresAt.HasValue && activationCode.ExpiresAt.Value < DateTime.UtcNow)
        {
            activationCode.Status = "Expired";
            await SaveDataToJsonAsync();
            return (false, 0, "This code has expired.");
        }

        // Activate the code
        activationCode.Status = "Used";
        activationCode.UsedAt = DateTime.UtcNow;

        await SaveDataToJsonAsync();

        System.Diagnostics.Debug.WriteLine($"[MockActivationCodesData] Code activated: {code} -> +{activationCode.DurationDays} days");
        return (true, activationCode.DurationDays, null);
    }

    /// <summary>
    /// Get all available (not used, not expired) codes
    /// </summary>
    public static async Task<List<ActivationCodeData>> GetAvailableCodesAsync()
    {
        EnsureDataLoaded();
        await Task.Delay(100);

        return _activationCodes!
            .Where(c => c.Status.Equals("Available", StringComparison.OrdinalIgnoreCase) &&
                       (!c.ExpiresAt.HasValue || c.ExpiresAt.Value > DateTime.UtcNow))
            .ToList();
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new ActivationCodesContainer
            {
                ActivationCodes = _activationCodes!,
                CodeHistory = _codeHistory!
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);
            
            System.Diagnostics.Debug.WriteLine("[MockActivationCodesData] Saved to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockActivationCodesData] Error saving JSON: {ex.Message}");
        }
    }

    #region Data Classes

    private class ActivationCodesContainer
    {
        public List<ActivationCodeData> ActivationCodes { get; set; } = new();
        public List<CodeHistoryData> CodeHistory { get; set; } = new();
    }

    public class ActivationCodeData
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "Trial"; // Trial, Professional, Enterprise
        public int DurationDays { get; set; } = 30;
        public int MaxUsers { get; set; } = 5;
        public List<string> Features { get; set; } = new();
        public string Status { get; set; } = "Available"; // Available, Used, Expired
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? UsedBy { get; set; }
        public DateTime? UsedAt { get; set; }
    }

    public class CodeHistoryData
    {
        public string UserId { get; set; } = string.Empty;
        public string CodeId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime ActivatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } = "Active";
    }

    #endregion
}
