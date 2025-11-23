using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Reports.Queries;

/// <summary>
/// Query to get sales reports (skeleton - backend not ready)
/// </summary>
public record GetSalesReportQuery(DateTimeOffset StartDate, DateTimeOffset EndDate) : IRequest<Result<object>>;
