using System.Text.Json;

namespace MyShop.Plugins.Repositories.Mocks.Data;

/// <summary>
/// Mock data provider for agent requests - loads from JSON file
/// </summary>
public static class MockAgentRequestsData
{
    private static List<MockAgentRequestData>? _agentRequests;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "Mocks", 
        "Data", 
        "Json", 
        "agent-requests.json"
    );

    private static void EnsureDataLoaded()
    {
        if (_agentRequests != null) return;

        lock (_lock)
        {
            if (_agentRequests != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsData] JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<AgentRequestsDataContainer>(jsonString, options);

                if (data?.AgentRequests != null)
                {
                    _agentRequests = data.AgentRequests;
                    System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsData] Loaded {_agentRequests.Count} agent requests from JSON");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsData] Error loading JSON: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _agentRequests = new List<MockAgentRequestData>
        {
            new MockAgentRequestData
            {
                Id = "40000000-0000-0000-0000-000000000001",
                UserId = "00000000-0000-0000-0000-000000000012",
                UserName = "Nguyễn Văn A",
                Email = "nguyen.van.a@example.com",
                PhoneNumber = "+84902000001",
                AvatarUrl = "ms-appx:///Assets/Images/avatars/avatar-placeholder.png",
                RequestedAt = DateTime.Parse("2025-11-20T09:30:00Z"),
                Status = "Pending",
                Notes = "Tôi muốn trở thành sales agent để giới thiệu sản phẩm cho bạn bè và gia đình"
            }
        };
        System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsData] Initialized with default data");
    }

    public static Task<IReadOnlyList<MockAgentRequestData>> GetAllAsync()
    {
        EnsureDataLoaded();
        return Task.FromResult<IReadOnlyList<MockAgentRequestData>>(_agentRequests!.AsReadOnly());
    }

    public static async Task<bool> ApproveAsync(Guid requestId, Guid reviewerId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var request = _agentRequests!.FirstOrDefault(r => r.Id == requestId.ToString());
        if (request == null || request.Status != "Pending")
        {
            return false;
        }

        request.Status = "Approved";
        request.ReviewedBy = reviewerId.ToString();
        request.ReviewedAt = DateTime.UtcNow;

        return true;
    }

    public static async Task<bool> RejectAsync(Guid requestId, Guid reviewerId, string reason)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var request = _agentRequests!.FirstOrDefault(r => r.Id == requestId.ToString());
        if (request == null || request.Status != "Pending")
        {
            return false;
        }

        request.Status = "Rejected";
        request.ReviewedBy = reviewerId.ToString();
        request.ReviewedAt = DateTime.UtcNow;
        request.Notes += $" | Rejection reason: {reason}";

        return true;
    }

    /// <summary>
    /// Container class for JSON deserialization
    /// </summary>
    private class AgentRequestsDataContainer
    {
        public List<MockAgentRequestData> AgentRequests { get; set; } = new();
    }

    /// <summary>
    /// Internal data model matching the JSON structure
    /// </summary>
    public class MockAgentRequestData
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; } = "Pending";    // Pending / Approved / Rejected
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string Notes { get; set; } = string.Empty;  // used as Reason/Experience text for UI
    }
}
