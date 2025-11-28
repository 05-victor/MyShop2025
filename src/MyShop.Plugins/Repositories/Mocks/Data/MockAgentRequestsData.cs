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
        // Initialize empty list - data should be loaded from agent-requests.json
        _agentRequests = new List<MockAgentRequestData>();
        System.Diagnostics.Debug.WriteLine("[MockAgentRequestsData] JSON file not found - initialized with empty agent requests list");
    }

    public static Task<IReadOnlyList<MockAgentRequestData>> GetAllAsync()
    {
        EnsureDataLoaded();
        return Task.FromResult<IReadOnlyList<MockAgentRequestData>>(_agentRequests!.AsReadOnly());
    }

    public static async Task<(List<MockAgentRequestData> Items, int TotalCount)> GetPagedAsync(
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "requestedAt",
        bool sortDescending = true)
    {
        EnsureDataLoaded();

        // Simulate minimal network delay for better UX
        await Task.Delay(50);

        var query = _agentRequests!.AsQueryable();

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        // Search filter - only search in notes since user data is enriched at repository layer
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerQuery = searchQuery.ToLower();
            query = query.Where(r => r.Notes.ToLower().Contains(lowerQuery));
        }

        // Sorting - only sort by request-specific fields; user-field sorting done at repository layer
        query = sortBy.ToLower() switch
        {
            "status" => sortDescending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "requestedat" => sortDescending
                ? query.OrderByDescending(r => r.RequestedAt)
                : query.OrderBy(r => r.RequestedAt),
            _ => sortDescending
                ? query.OrderByDescending(r => r.RequestedAt)
                : query.OrderBy(r => r.RequestedAt)
        };

        var totalCount = query.Count();
        var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return (pagedData, totalCount);
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

    public static async Task<MyShop.Shared.Models.AgentRequest?> CreateAsync(MyShop.Shared.Models.AgentRequest agentRequest)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        try
        {
            // Only store request-specific fields; user info is fetched from users.json by userId
            var newMockRequest = new MockAgentRequestData
            {
                Id = agentRequest.Id.ToString(),
                UserId = agentRequest.UserId.ToString(),
                RequestedAt = agentRequest.RequestedAt,
                Status = string.IsNullOrWhiteSpace(agentRequest.Status) ? "Pending" : agentRequest.Status,
                ReviewedBy = agentRequest.ReviewedBy?.ToString(),
                ReviewedAt = agentRequest.ReviewedAt,
                Notes = agentRequest.Notes
            };

            _agentRequests!.Add(newMockRequest);

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsData] Created new agent request: {agentRequest.Id}");

            return agentRequest;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsData] CreateAsync error: {ex.Message}");
            return null;
        }
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
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; } = "Pending";    // Pending / Approved / Rejected
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string Notes { get; set; } = string.Empty;  // reason/experience combined or admin notes
    }
}
