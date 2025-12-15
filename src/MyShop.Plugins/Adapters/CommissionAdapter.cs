using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Adapters;

/// <summary>
/// Adapter for mapping Commission DTOs to domain models
/// </summary>
public static class CommissionAdapter
{
    /// <summary>
    /// Maps CommissionResponse DTO to Commission model
    /// </summary>
    public static Commission ToModel(CommissionResponse dto)
    {
        return new Commission
        {
            Id = dto.Id,
            OrderId = dto.OrderId,
            SalesAgentId = dto.SalesAgentId,
            OrderNumber = dto.OrderNumber,
            OrderAmount = dto.OrderAmount,
            CommissionRate = dto.CommissionRate,
            CommissionAmount = dto.CommissionAmount,
            Amount = dto.CommissionAmount, // Alias
            Status = dto.Status,
            CreatedDate = dto.CreatedDate,
            CreatedAt = dto.CreatedDate, // Alias
            PaidDate = dto.PaidDate,
            PaidAt = dto.PaidDate // Alias
        };
    }

    /// <summary>
    /// Maps CommissionSummaryResponse DTO to CommissionSummary model
    /// </summary>
    public static CommissionSummary ToModel(CommissionSummaryResponse dto)
    {
        return new CommissionSummary
        {
            TotalEarnings = dto.TotalEarnings,
            PendingCommission = dto.PendingCommission,
            PaidCommission = dto.PaidCommission,
            TotalOrders = dto.TotalOrders,
            AverageCommission = dto.AverageCommission,
            ThisMonthEarnings = dto.ThisMonthEarnings,
            LastMonthEarnings = dto.LastMonthEarnings
        };
    }

    /// <summary>
    /// Maps list of CommissionResponse DTOs to list of Commission models
    /// </summary>
    public static List<Commission> ToModelList(IEnumerable<CommissionResponse> dtos)
    {
        return dtos.Select(ToModel).ToList();
    }
}
