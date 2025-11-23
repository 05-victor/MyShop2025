using MediatR;
using MyShop.Client.Features.Reports.Queries;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Reports.Handlers;

/// <summary>
/// Handler for GetSalesReportQuery (skeleton - backend not ready)
/// </summary>
public class GetSalesReportHandler : IRequestHandler<GetSalesReportQuery, Result<object>>
{
    public Task<Result<object>> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Backend API not ready");
    }
}
