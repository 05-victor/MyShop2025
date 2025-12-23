using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Users;

/// <summary>
/// Refit interface for Agent Requests API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IAgentRequestsApi
{
    /// <summary>
    /// Create a new agent request (User sends request to become SalesAgent)
    /// </summary>
    [Post("/api/v1/agent-requests")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<AgentRequestResponse>>> CreateAsync([Body] CreateAgentRequestRequest request);

    /// <summary>
    /// Get all agent requests with pagination (Admin only)
    /// </summary>
    [Get("/api/v1/agent-requests")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<PagedResult<AgentRequestResponse>>>> GetAllAsync(
        [Query] int pageNumber = 1,
        [Query] int pageSize = 10,
        [Query] string? status = null);

    /// <summary>
    /// Get current user's agent request
    /// </summary>
    [Get("/api/v1/agent-requests/my-request")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<AgentRequestResponse>>> GetMyRequestAsync();

    /// <summary>
    /// Get agent request by ID (Admin only)
    /// </summary>
    [Get("/api/v1/agent-requests/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<AgentRequestResponse>>> GetByIdAsync(Guid id);

    /// <summary>
    /// Approve an agent request (Admin only) - Promotes user to SalesAgent role
    /// </summary>
    [Patch("/api/v1/agent-requests/{id}/approve")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> ApproveAsync(Guid id);

    /// <summary>
    /// Reject an agent request (Admin only)
    /// </summary>
    [Patch("/api/v1/agent-requests/{id}/reject")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> RejectAsync(Guid id, [Body] object rejectRequest);
}
