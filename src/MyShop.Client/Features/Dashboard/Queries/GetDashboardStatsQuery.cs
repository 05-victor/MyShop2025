using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Dashboard.Queries;

/// <summary>
/// Query to get dashboard statistics (skeleton - backend not ready)
/// </summary>
public record GetDashboardStatsQuery() : IRequest<Result<object>>;
