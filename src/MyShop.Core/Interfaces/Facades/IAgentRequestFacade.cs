using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for sales agent request management.
/// Aggregates: IAgentRequestRepository, IUserRepository, IValidationService, IToastService.
/// Handles agent application submission, approval/rejection workflow, and status management.
/// </summary>
public interface IAgentRequestFacade
{
    /// <summary>
    /// Load agent requests with paging and filtering.
    /// </summary>
    Task<Result<PagedList<AgentRequest>>> LoadRequestsAsync(
        string? status = null,
        string? searchQuery = null,
        int page = 1,
        int pageSize = Common.PaginationConstants.AgentRequestsPageSize);

    /// <summary>
    /// Get paged agent requests (server-side)
    /// </summary>
    Task<Result<PagedList<AgentRequest>>> GetPagedAsync(
        int page = 1,
        int pageSize = Common.PaginationConstants.AgentRequestsPageSize,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "requestedAt",
        bool sortDescending = true);

    /// <summary>
    /// Get request by ID
    /// </summary>
    Task<Result<AgentRequest>> GetRequestByIdAsync(Guid requestId);

    /// <summary>
    /// Submit new agent request (Customer submits application)
    /// </summary>
    Task<Result<AgentRequest>> SubmitRequestAsync(
        string reason,
        string experience,
        string fullName,
        string email,
        string phoneNumber,
        string address,
        string? businessName = null,
        string? taxId = null);

    /// <summary>
    /// Approve agent request (admin only)
    /// </summary>
    Task<Result<Unit>> ApproveRequestAsync(Guid requestId, string? notes = null);

    /// <summary>
    /// Reject agent request (admin only)
    /// </summary>
    Task<Result<Unit>> RejectRequestAsync(Guid requestId, string reason);

    /// <summary>
    /// Get pending requests count
    /// </summary>
    Task<Result<int>> GetPendingRequestsCountAsync();

    /// <summary>
    /// Get current user's agent request (if exists)
    /// </summary>
    Task<Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>> GetMyRequestAsync();

    /// <summary>
    /// View user profile for request
    /// </summary>
    Task<Result<User>> GetRequestUserProfileAsync(Guid requestId);
}
