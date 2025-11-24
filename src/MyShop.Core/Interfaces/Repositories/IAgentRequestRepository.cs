using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing agent requests
/// </summary>
public interface IAgentRequestRepository
{
    /// <summary>
    /// Get all agent requests
    /// </summary>
    Task<IEnumerable<AgentRequest>> GetAllAsync();

    /// <summary>
    /// Approve an agent request
    /// </summary>
    Task<bool> ApproveAsync(Guid id);

    /// <summary>
    /// Reject an agent request
    /// </summary>
    Task<bool> RejectAsync(Guid id);
}
