using MyShop.Shared.Models;
using MyShop.Core.Common;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing agent requests
/// </summary>
public interface IAgentRequestRepository
{
    /// <summary>
    /// Get all agent requests
    /// </summary>
    Task<Result<IEnumerable<AgentRequest>>> GetAllAsync();

    /// <summary>
    /// Get paged agent requests with filtering
    /// </summary>
    Task<Result<PagedList<AgentRequest>>> GetPagedAsync(
        int page = 1,
        int pageSize = Common.PaginationConstants.AgentRequestsPageSize,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "requestedAt",
        bool sortDescending = true);

    /// <summary>
    /// Create new agent request
    /// </summary>
    Task<Result<AgentRequest>> CreateAsync(AgentRequest agentRequest);

    /// <summary>
    /// Approve an agent request
    /// </summary>
    Task<Result<bool>> ApproveAsync(Guid id);

    /// <summary>
    /// Reject an agent request
    /// </summary>
    Task<Result<bool>> RejectAsync(Guid id);
}
