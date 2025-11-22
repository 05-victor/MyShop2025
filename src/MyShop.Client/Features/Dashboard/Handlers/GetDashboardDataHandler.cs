using MediatR;
using MyShop.Client.Features.Dashboard.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Features.Dashboard.Handlers;

/// <summary>
/// Handler for GetDashboardDataQuery
/// </summary>
public class GetDashboardDataHandler : IRequestHandler<GetDashboardDataQuery, Result<object>>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardDataHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Result<object>> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _dashboardRepository.GetSummaryAsync();
            return Result<object>.Success(data);
        }
        catch (Exception ex)
        {
            return Result<object>.Failure($"Error loading dashboard data: {ex.Message}");
        }
    }
}
