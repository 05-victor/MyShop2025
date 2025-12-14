using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IAgentRequestService
{
    Task<AgentRequestResponse?> GetByIdAsync(Guid id);
    Task<AgentRequestResponse?> GetMyRequestAsync();
    Task<PagedResult<AgentRequestResponse>> GetAllAsync(PaginationRequest request, string? status = null);
    Task<AgentRequestResponse> CreateAsync(CreateAgentRequestRequest request);
    Task<ActivateUserResponse> ApproveAsync(Guid id);
    Task<ActivateUserResponse> RejectAsync(Guid id, RejectAgentRequest rejectAgentRequest);
}
