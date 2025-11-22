using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Dashboard.Queries;

/// <summary>
/// Query to get dashboard data (skeleton - backend not ready)
/// </summary>
public record GetDashboardDataQuery() : IRequest<Result<object>>;
