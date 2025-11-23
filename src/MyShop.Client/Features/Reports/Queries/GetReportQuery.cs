using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Reports.Queries;

/// <summary>
/// Query to get report data (skeleton - backend not ready)
/// </summary>
public record GetReportQuery(string ReportType, DateTimeOffset? StartDate, DateTimeOffset? EndDate) : IRequest<Result<object>>;
