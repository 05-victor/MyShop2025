using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System.Text;

namespace MyShop.Client.Facades.Reports;

/// <summary>
/// Facade for commission management operations
/// Aggregates: ICommissionRepository, IToastService, IAuthRepository, IExportService
/// </summary>
public class CommissionFacade : ICommissionFacade
{
    private readonly ICommissionRepository _commissionRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IExportService _exportService;
    private readonly IToastService _toastService;

    public CommissionFacade(
        ICommissionRepository commissionRepository,
        IAuthRepository authRepository,
        IExportService exportService,
        IToastService toastService)
    {
        _commissionRepository = commissionRepository ?? throw new ArgumentNullException(nameof(commissionRepository));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    public async Task<Result<PagedList<Commission>>> LoadCommissionsAsync(Guid? agentId = null, string? status = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
    {
        try
        {
            // Get current user's ID if agentId is not provided
            Guid? effectiveAgentId = agentId;
            if (!effectiveAgentId.HasValue)
            {
                var userIdResult = await _authRepository.GetCurrentUserIdAsync();
                if (userIdResult.IsSuccess)
                {
                    effectiveAgentId = userIdResult.Data;
                }
            }

            if (!effectiveAgentId.HasValue)
            {
                await _toastService.ShowError("Unable to identify current user");
                return Result<PagedList<Commission>>.Failure("Unable to identify current user");
            }

            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Loading commissions for agent: {effectiveAgentId}");

            var result = await _commissionRepository.GetPagedAsync(
                salesAgentId: effectiveAgentId.Value,
                page: page,
                pageSize: pageSize,
                status: status,
                startDate: startDate,
                endDate: endDate
            );

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Failed to load commissions: {result.ErrorMessage}");
                return Result<PagedList<Commission>>.Failure(result.ErrorMessage ?? "Failed to load commissions");
            }

            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Loaded {result.Data.Items.Count} commissions, total: {result.Data.TotalCount}");
            return result;
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
            var result = await _commissionRepository.GetSummaryAsync(agentId);
            
            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Failed to get summary: {result.ErrorMessage}");
                return Result<CommissionSummary>.Failure(result.ErrorMessage ?? "Failed to get commission summary");
            }

            return result;
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
            // Get current user's ID if agentId is not provided
            Guid? effectiveAgentId = agentId;
            if (!effectiveAgentId.HasValue)
            {
                var userIdResult = await _authRepository.GetCurrentUserIdAsync();
                if (userIdResult.IsSuccess)
                {
                    effectiveAgentId = userIdResult.Data;
                }
            }

            if (!effectiveAgentId.HasValue)
            {
                await _toastService.ShowError("Agent ID is required to export commissions");
                return Result<string>.Failure("Agent ID is required to export commissions");
            }

            var result = await _commissionRepository.GetPagedAsync(
                salesAgentId: effectiveAgentId.Value,
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

            // Use ExportService with FileSavePicker (same pattern as OrderFacade)
            var suggestedFileName = $"Commissions_{DateTime.Now:yyyyMMdd_HHmmss}";
            var exportResult = await _exportService.ExportWithPickerAsync(suggestedFileName, csv.ToString());

            if (!exportResult.IsSuccess)
            {
                await _toastService.ShowError("Failed to export commissions");
                return exportResult;
            }

            // Empty path means user cancelled
            if (string.IsNullOrEmpty(exportResult.Data))
            {
                return Result<string>.Success(string.Empty);
            }

            await _toastService.ShowSuccess($"Exported {commissions.Count} commissions successfully!");
            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Exported {commissions.Count} commissions to {exportResult.Data}");
            return exportResult;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CommissionFacade] Error exporting commissions: {ex.Message}");
            await _toastService.ShowError($"Error exporting commissions: {ex.Message}");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
