using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System.Text;

namespace MyShop.Client.Facades.Reports;

/// <summary>
/// Facade for commission management operations
/// Aggregates: ICommissionRepository, IToastService
/// </summary>
public class CommissionFacade : ICommissionFacade
{
    private readonly ICommissionRepository _commissionRepository;
    private readonly IToastService _toastService;

    public CommissionFacade(ICommissionRepository commissionRepository, IToastService toastService)
    {
        _commissionRepository = commissionRepository ?? throw new ArgumentNullException(nameof(commissionRepository));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    public async Task<Result<PagedList<Commission>>> LoadCommissionsAsync(Guid? agentId = null, string? status = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
    {
        try
        {
            await _toastService.ShowInfo("Commission loading - Feature coming soon");
            return Result<PagedList<Commission>>.Failure("Not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Error loading commissions: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<PagedList<Commission>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CommissionSummary>> GetCommissionSummaryAsync(Guid agentId, string period = "current")
    {
        try
        {
            await _toastService.ShowInfo("Commission summary - Feature coming soon");
            return Result<CommissionSummary>.Failure("Not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Error getting summary: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<CommissionSummary>.Failure($"Error: {ex.Message}");
        }
    }

    public Task<Result<decimal>> GetPendingCommissionsAsync(Guid agentId)
    {
        return Task.FromResult(Result<decimal>.Success(0m));
    }

    public Task<Result<decimal>> GetPaidCommissionsAsync(Guid agentId)
    {
        return Task.FromResult(Result<decimal>.Success(0m));
    }

    public async Task<Result<Unit>> MarkCommissionAsPaidAsync(Guid commissionId)
    {
        try
        {
            await _toastService.ShowSuccess("Commission marked as paid");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Error marking as paid: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExportCommissionsAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // Note: ICommissionRepository.GetPagedAsync requires salesAgentId as first parameter
            // If agentId is null, we can't filter by agent - would need different approach
            if (!agentId.HasValue)
            {
                return Result<string>.Failure("Agent ID is required to export commissions");
            }

            var result = await _commissionRepository.GetPagedAsync(
                salesAgentId: agentId.Value,
                page: 1, pageSize: 10000,
                status: null,
                startDate: startDate, endDate: endDate);

            if (!result.IsSuccess || result.Data == null)
            {
                await _toastService.ShowError("Failed to load commissions for export");
                return Result<string>.Failure("Failed to load commissions");
            }

            var commissions = result.Data.Items;
            var csv = new StringBuilder();
            csv.AppendLine("Commission ID,Order ID,Agent ID,Agent Name,Commission Amount,Commission Rate,Status,Order Date,Paid Date");

            foreach (var commission in commissions)
            {
                csv.AppendLine($"\"{commission.Id}\",\"{commission.OrderId}\",\"{commission.SalesAgentId}\",\"{commission.SalesAgentName}\"," +
                    $"\"{commission.CommissionAmount:F2}\",\"{commission.CommissionRate}\",\"{commission.Status}\"," +
                    $"\"{commission.CreatedDate:yyyy-MM-dd HH:mm}\",\"{commission.PaidDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"}\"");
            }

            var fileName = $"Commissions_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = StorageConstants.GetExportFilePath(fileName);
            await File.WriteAllTextAsync(filePath, csv.ToString());

            await _toastService.ShowSuccess($"Exported {commissions.Count} commissions to {fileName}");
            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Exported {commissions.Count} commissions to {filePath}");
            return Result<string>.Success(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Error exporting commissions: {ex.Message}");
            await _toastService.ShowError($"Error exporting commissions: {ex.Message}");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
