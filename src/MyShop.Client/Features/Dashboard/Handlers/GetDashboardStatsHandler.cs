using MediatR;
using MyShop.Client.Features.Dashboard.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Features.Dashboard.Handlers;

/// <summary>
/// Handler for GetDashboardStatsQuery
/// </summary>
public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Result<object>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardStatsHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Result<object>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _dashboardRepository.GetSummaryAsync();
            return Result<object>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<object>.Failure($"Error loading dashboard stats: {ex.Message}");
        }
    }
}
