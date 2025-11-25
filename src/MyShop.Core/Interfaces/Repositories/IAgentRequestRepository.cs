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
    /// Approve an agent request
    /// </summary>
    Task<Result<bool>> ApproveAsync(Guid id);

    /// <summary>
    /// Reject an agent request
    /// </summary>
    Task<Result<bool>> RejectAsync(Guid id);
}
