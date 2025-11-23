using System.Text.Json;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for Commission management using generated data
/// </summary>
public class MockCommissionRepository : ICommissionRepository
{
    private readonly List<Commission> _commissions;

    public MockCommissionRepository()
    {
        _commissions = GenerateMockCommissions();
    }

    private List<Commission> GenerateMockCommissions()
    {
        var commissions = new List<Commission>();
        var random = new Random(42); // Fixed seed for consistent data

        // Mock sales agent IDs
        var salesAgentIds = new[]
        {
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Guid.Parse("00000000-0000-0000-0000-000000000003")
        };

        // Generate 50 commission records
        for (int i = 0; i < 50; i++)
        {
            var orderAmount = (decimal)(random.NextDouble() * 5000 + 100); // $100 - $5100
            var commissionRate = 10m; // 10% commission
            var status = random.Next(0, 3) switch
            {
                0 => "Pending",
                1 => "Approved",
                _ => "Paid"
            };

            var createdDate = DateTime.Now.AddDays(-random.Next(1, 90));
            var paidDate = status == "Paid" ? createdDate.AddDays(random.Next(7, 30)) : (DateTime?)null;

            commissions.Add(new Commission
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                SalesAgentId = salesAgentIds[random.Next(salesAgentIds.Length)],
                OrderNumber = $"ORD-{10000 + i}",
                OrderAmount = orderAmount,
                CommissionRate = commissionRate,
                CommissionAmount = Math.Round(orderAmount * commissionRate / 100, 2),
                Status = status,
                CreatedDate = createdDate,
                PaidDate = paidDate
            });
        }

        return commissions.OrderByDescending(c => c.CreatedDate).ToList();
    }

    public Task<IEnumerable<Commission>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        var commissions = _commissions.Where(c => c.SalesAgentId == salesAgentId);
        return Task.FromResult(commissions);
    }

    public Task<CommissionSummary> GetSummaryAsync(Guid salesAgentId)
    {
        var agentCommissions = _commissions.Where(c => c.SalesAgentId == salesAgentId).ToList();

        if (!agentCommissions.Any())
        {
            return Task.FromResult(new CommissionSummary());
        }

        var now = DateTime.Now;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var summary = new CommissionSummary
        {
            TotalEarnings = agentCommissions.Sum(c => c.CommissionAmount),
            PendingCommission = agentCommissions.Where(c => c.Status == "Pending").Sum(c => c.CommissionAmount),
            PaidCommission = agentCommissions.Where(c => c.Status == "Paid").Sum(c => c.CommissionAmount),
            TotalOrders = agentCommissions.Count,
            AverageCommission = agentCommissions.Average(c => c.CommissionAmount),
            ThisMonthEarnings = agentCommissions
                .Where(c => c.CreatedDate >= thisMonthStart)
                .Sum(c => c.CommissionAmount),
            LastMonthEarnings = agentCommissions
                .Where(c => c.CreatedDate >= lastMonthStart && c.CreatedDate < thisMonthStart)
                .Sum(c => c.CommissionAmount)
        };

        return Task.FromResult(summary);
    }

    public Task<Commission?> GetByOrderIdAsync(Guid orderId)
    {
        var commission = _commissions.FirstOrDefault(c => c.OrderId == orderId);
        return Task.FromResult(commission);
    }

    public Task<decimal> CalculateCommissionAsync(Guid orderId)
    {
        var commission = _commissions.FirstOrDefault(c => c.OrderId == orderId);
        if (commission == null)
        {
            return Task.FromResult(0m);
        }

        // Calculate commission: orderAmount * rate%
        var calculatedAmount = commission.OrderAmount * commission.CommissionRate / 100;
        return Task.FromResult(Math.Round(calculatedAmount, 2));
    }

    public Task<IEnumerable<Commission>> GetByDateRangeAsync(Guid salesAgentId, DateTime startDate, DateTime endDate)
    {
        var commissions = _commissions
            .Where(c => c.SalesAgentId == salesAgentId &&
                        c.CreatedDate >= startDate &&
                        c.CreatedDate <= endDate)
            .OrderByDescending(c => c.CreatedDate);

        return Task.FromResult(commissions.AsEnumerable());
    }
}
