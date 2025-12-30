using MyShop.Core.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for forecast operations
/// </summary>
public interface IForecastRepository
{
    /// <summary>
    /// Predict weekly revenue for the current SalesAgent's department
    /// </summary>
    /// <param name="date">Date for prediction (format: yyyy-mm-dd)</param>
    /// <returns>Price forecast response with predicted weekly sales in USD</returns>
    Task<Result<PriceForecastResponse>> PredictRevenueAsync(string date);

    /// <summary>
    /// Predict product demand for the current SalesAgent's store
    /// </summary>
    /// <param name="request">Demand forecast request</param>
    /// <returns>Demand forecast response with predicted units</returns>
    Task<Result<DemandForecastResponse>> PredictDemandAsync(DemandForecastRequest request);
}
