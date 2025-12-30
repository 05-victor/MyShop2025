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
    /// Get agent request by ID
    /// </summary>
    Task<Result<AgentRequest>> GetByIdAsync(Guid id);

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
    /// Get user's own agent request
    /// </summary>
    Task<Result<AgentRequest>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Get current user's agent request (returns DTO from API)
    /// </summary>
    Task<Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>> GetMyRequestAsync();

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
    Task<Result<bool>> RejectAsync(Guid id, string reason = "");
}
