using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for commissions - loads from JSON file (if exists)
/// </summary>
public static class MockCommissionData
{
    private static List<CommissionDataModel>? _commissions;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "commissions.json");

    private static void EnsureDataLoaded()
    {
        if (_commissions != null) return;

        lock (_lock)
        {
            if (_commissions != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Commissions JSON file not found at: {_jsonFilePath} - using default data");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<CommissionDataContainer>(jsonString, options);

                if (data?.Commissions != null)
                {
                    _commissions = data.Commissions;
                    System.Diagnostics.Debug.WriteLine($"Loaded {_commissions.Count} commissions from commissions.json");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading commissions.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        // Initialize empty list - data should be loaded from commissions.json
        _commissions = new List<CommissionDataModel>();
        System.Diagnostics.Debug.WriteLine("[MockCommissionData] JSON file not found - initialized with empty commission list");
    }

    public static async Task<List<Commission>> GetAllAsync()
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(350);

        return _commissions!.Select(MapToCommission).ToList();
    }

    public static async Task<Commission?> GetByIdAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(200);

        var commissionData = _commissions!.FirstOrDefault(c => c.Id == id.ToString());
        if (commissionData == null) return null;

        return MapToCommission(commissionData);
    }

    public static async Task<List<Commission>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        return _commissions!
            .Where(c => c.SalesAgentId == salesAgentId.ToString())
            .Select(MapToCommission)
            .ToList();
    }

    public static async Task<decimal> GetTotalEarnedAsync(Guid salesAgentId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(250);

        return _commissions!
            .Where(c => c.SalesAgentId == salesAgentId.ToString() && c.Status == "PAID")
            .Sum(c => c.Amount);
    }

    public static async Task<CommissionSummary> GetSummaryAsync(Guid salesAgentId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var agentCommissions = _commissions!
            .Where(c => c.SalesAgentId == salesAgentId.ToString())
            .Select(MapToCommission)
            .ToList();

        if (agentCommissions.Count == 0)
        {
            return new CommissionSummary();
        }

        var now = DateTime.Now;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        return new CommissionSummary
        {
            TotalEarnings = agentCommissions.Sum(c => c.Amount),
            PendingCommission = agentCommissions.Where(c => c.Status == "Pending").Sum(c => c.Amount),
            PaidCommission = agentCommissions.Where(c => c.Status == "Paid").Sum(c => c.Amount),
            TotalOrders = agentCommissions.Count,
            AverageCommission = agentCommissions.Average(c => c.Amount),
            ThisMonthEarnings = agentCommissions
                .Where(c => c.CreatedAt >= thisMonthStart)
                .Sum(c => c.Amount),
            LastMonthEarnings = agentCommissions
                .Where(c => c.CreatedAt >= lastMonthStart && c.CreatedAt < thisMonthStart)
                .Sum(c => c.Amount)
        };
    }

    public static async Task<Commission?> GetByOrderIdAsync(Guid orderId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(200);

        var commissionData = _commissions!.FirstOrDefault(c => c.OrderId == orderId.ToString());
        if (commissionData == null) return null;

        return MapToCommission(commissionData);
    }

    public static async Task<decimal> CalculateCommissionAsync(Guid orderId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(200);

        var commission = _commissions!.FirstOrDefault(c => c.OrderId == orderId.ToString());
        if (commission == null) return 0m;

        // Assume 10% commission rate
        return Math.Round(commission.Amount, 2);
    }

    public static async Task<List<Commission>> GetByDateRangeAsync(Guid salesAgentId, DateTime startDate, DateTime endDate)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        return _commissions!
            .Where(c => c.SalesAgentId == salesAgentId.ToString() &&
                        c.CreatedAt >= startDate &&
                        c.CreatedAt <= endDate)
            .Select(MapToCommission)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    public static async Task<(List<Commission> Items, int TotalCount)> GetPagedAsync(
        Guid salesAgentId,
        int page = 1,
        int pageSize = 20,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "createdDate",
        bool sortDescending = true)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var query = _commissions!
            .Where(c => c.SalesAgentId == salesAgentId.ToString())
            .AsEnumerable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "createddate" or "createdat" => sortDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            "amount" => sortDescending
                ? query.OrderByDescending(c => c.Amount)
                : query.OrderBy(c => c.Amount),
            "status" => sortDescending
                ? query.OrderByDescending(c => c.Status)
                : query.OrderBy(c => c.Status),
            "paiddate" or "paidat" => sortDescending
                ? query.OrderByDescending(c => c.PaidAt ?? DateTime.MinValue)
                : query.OrderBy(c => c.PaidAt ?? DateTime.MinValue),
            _ => sortDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt)
        };

        var totalCount = query.Count();
        var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(MapToCommission)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"[MockCommissionData] GetPagedAsync: Page {page}/{Math.Ceiling(totalCount / (double)pageSize)}, Total: {totalCount}");

        return (pagedData, totalCount);
    }

    private static Commission MapToCommission(CommissionDataModel data)
    {
        return new Commission
        {
            Id = Guid.Parse(data.Id),
            SalesAgentId = Guid.Parse(data.SalesAgentId),
            SalesAgentName = data.SalesAgentName,
            OrderId = Guid.Parse(data.OrderId),
            Amount = data.Amount,
            CommissionAmount = data.Amount, // Same value
            Status = data.Status,
            CreatedAt = data.CreatedAt,
            CreatedDate = data.CreatedAt, // Same value
            PaidAt = data.PaidAt,
            PaidDate = data.PaidAt // Same value
        };
    }

    // Data container classes for JSON deserialization
    private class CommissionDataContainer
    {
        public List<CommissionDataModel> Commissions { get; set; } = new();
    }

    private class CommissionDataModel
    {
        public string Id { get; set; } = string.Empty;
        public string SalesAgentId { get; set; } = string.Empty;
        public string? SalesAgentName { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "PENDING";
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
