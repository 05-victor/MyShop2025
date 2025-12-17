using Microsoft.Extensions.Configuration;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Enums;
using MyShop.Shared.Extensions;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service for managing sales agent earnings
/// Calculates earnings, platform fees, and net income
/// </summary>
public class EarningsService : IEarningsService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EarningsService> _logger;
    private readonly decimal _platformFeeRate;

    public EarningsService(
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        ILogger<EarningsService> logger)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _configuration = configuration;
        _logger = logger;

        // Read platform fee from configuration (default to 10%)
        _platformFeeRate = _configuration.GetValue<decimal>("BusinessSettings:PlatformFee", 0.10m);
        _logger.LogInformation("EarningsService initialized with platform fee rate: {PlatformFeeRate:P}", _platformFeeRate);
    }

    public async Task<EarningsSummaryResponse> GetMySummaryAsync()
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var salesAgentId = currentUserId.Value;
            _logger.LogInformation("Calculating earnings summary for sales agent {SalesAgentId}", salesAgentId);

            // Get all orders for this sales agent
            var pagedOrders = await _orderRepository.GetOrdersBySalesAgentIdAsync(1, int.MaxValue, salesAgentId);
            var orders = pagedOrders.Items.ToList();

            if (orders.Count == 0)
            {
                _logger.LogInformation("No orders found for sales agent {SalesAgentId}", salesAgentId);
                return new EarningsSummaryResponse
                {
                    PlatformFeeRate = _platformFeeRate
                };
            }

            // Calculate total earnings (sum of all order grand totals)
            var totalEarnings = orders.Sum(o => o.GrandTotal);

            // Calculate platform fees
            var totalPlatformFees = totalEarnings * _platformFeeRate;

            // Calculate net earnings
            var netEarnings = totalEarnings - totalPlatformFees;

            // Separate paid and pending earnings
            var paidOrders = orders.Where(o => o.PaymentStatus == PaymentStatus.Paid).ToList();
            var paidEarnings = paidOrders.Sum(o => o.GrandTotal) * (1 - _platformFeeRate);

            var pendingOrders = orders.Where(o => o.PaymentStatus != PaymentStatus.Paid 
                                                 && o.Status != OrderStatus.Cancelled).ToList();
            var pendingEarnings = pendingOrders.Sum(o => o.GrandTotal) * (1 - _platformFeeRate);

            // Calculate last month earnings
            var now = DateTime.UtcNow;
            var lastMonthStart = now.AddMonths(-1).StartOfMonth();
            var lastMonthEnd = lastMonthStart.EndOfMonth();
            var lastMonthOrders = orders.Where(o => o.OrderDate >= lastMonthStart && o.OrderDate <= lastMonthEnd).ToList();
            var lastMonthEarnings = lastMonthOrders.Sum(o => o.GrandTotal) * (1 - _platformFeeRate);

            // Calculate this month earnings
            var thisMonthStart = now.StartOfMonth();
            var thisMonthEnd = now.EndOfMonth();
            var thisMonthOrders = orders.Where(o => o.OrderDate >= thisMonthStart && o.OrderDate <= thisMonthEnd).ToList();
            var thisMonthEarnings = thisMonthOrders.Sum(o => o.GrandTotal) * (1 - _platformFeeRate);

            // Calculate average earnings per order
            var averageEarningsPerOrder = orders.Count > 0 
                ? netEarnings / orders.Count 
                : 0;

            var summary = new EarningsSummaryResponse
            {
                TotalEarnings = totalEarnings,
                TotalPlatformFees = totalPlatformFees,
                NetEarnings = netEarnings,
                PendingEarnings = pendingEarnings,
                PaidEarnings = paidEarnings,
                TotalOrders = orders.Count,
                AverageEarningsPerOrder = averageEarningsPerOrder,
                ThisMonthEarnings = thisMonthEarnings,
                LastMonthEarnings = lastMonthEarnings,
                PlatformFeeRate = _platformFeeRate
            };

            _logger.LogInformation(
                "Earnings summary calculated for agent {SalesAgentId}: Total={Total}, Net={Net}, Orders={Count}",
                salesAgentId, totalEarnings, netEarnings, orders.Count);

            return summary;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating earnings summary");
            throw;
        }
    }

    public async Task<PagedResult<EarningHistoryResponse>> GetMyHistoryAsync(
        PaginationRequest request,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null,
        string? paymentStatus = null)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var salesAgentId = currentUserId.Value;
            _logger.LogInformation(
                "Loading earnings history for sales agent {SalesAgentId}, Page={Page}, PageSize={PageSize}",
                salesAgentId, request.PageNumber, request.PageSize);

            // Ensure dates are UTC if provided
            var utcStartDate = startDate?.EnsureUtc();
            var utcEndDate = endDate?.EnsureUtc();

            // Use repository method to get filtered orders directly from database
            var pagedOrders = await _orderRepository.GetFilteredOrdersBySalesAgentAsync(
                salesAgentId: salesAgentId,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                startDate: utcStartDate,
                endDate: utcEndDate,
                status: status,
                paymentStatus: paymentStatus,
                sortBy: "OrderDate",
                sortDescending: true);

            // Map to response DTOs
            var historyItems = pagedOrders.Items.Select(order =>
            {
                var orderAmount = order.GrandTotal;
                var platformFee = orderAmount * _platformFeeRate;
                var netEarnings = orderAmount - platformFee;

                return new EarningHistoryResponse
                {
                    OrderId = order.Id,
                    OrderCode = GenerateOrderCode(order.Id),
                    CustomerName = order.Customer?.Username ?? "Unknown",
                    OrderDate = order.OrderDate,
                    OrderStatus = order.Status.ToApiString(),
                    PaymentStatus = order.PaymentStatus.ToApiString(),
                    OrderAmount = orderAmount,
                    PlatformFee = platformFee,
                    NetEarnings = netEarnings
                };
            }).ToList();

            _logger.LogInformation(
                "Loaded {Count} earnings records (page {Page}/{TotalPages})",
                historyItems.Count, request.PageNumber, (int)Math.Ceiling((double)pagedOrders.TotalCount / request.PageSize));

            return new PagedResult<EarningHistoryResponse>
            {
                Items = historyItems,
                TotalCount = pagedOrders.TotalCount,
                Page = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading earnings history");
            throw;
        }
    }

    /// <summary>
    /// Generate a readable order code from order ID
    /// </summary>
    private static string GenerateOrderCode(Guid orderId)
    {
        return $"ORD-{orderId.ToString().Substring(0, 8).ToUpper()}";
    }
}
