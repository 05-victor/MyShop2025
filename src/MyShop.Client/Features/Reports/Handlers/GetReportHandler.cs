using MediatR;
using MyShop.Client.Features.Reports.Queries;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Reports.Handlers;

/// <summary>
/// Handler for GetReportQuery (skeleton - backend not ready)
/// </summary>
public class GetReportHandler : IRequestHandler<GetReportQuery, Result<object>>
{
    public Task<Result<object>> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Backend API not ready");
    }
}
