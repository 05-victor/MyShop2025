using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.Enums;

namespace MyShop.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for AgentRequest operations in the Data layer
/// </summary>
public interface IAgentRequestRepository
{
    Task<AgentRequest?> GetByIdAsync(Guid id);
    Task<AgentRequest?> GetByUserIdAsync(Guid userId);
    Task<PagedResult<AgentRequest>> GetAllAsync(int pageNumber = 1, int pageSize = 20, AgentRequestStatus? status = null);
    Task<AgentRequest> CreateAsync(AgentRequest agentRequest);
    Task<AgentRequest> UpdateAsync(AgentRequest agentRequest);
    Task<bool> DeleteAsync(Guid id);
}
