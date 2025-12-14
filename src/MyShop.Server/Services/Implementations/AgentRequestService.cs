using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Mappings;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

public class AgentRequestService : IAgentRequestService
{
    private readonly IAgentRequestRepository _agentRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AgentRequestService> _logger;

    public AgentRequestService(
        IAgentRequestRepository agentRequestRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICurrentUserService currentUserService,
        ILogger<AgentRequestService> logger)
    {
        _agentRequestRepository = agentRequestRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<AgentRequestResponse?> GetByIdAsync(Guid id)
    {
        var request = await _agentRequestRepository.GetByIdAsync(id);
        return request == null ? null : AgentRequestMapper.ToAgentRequestResponse(request);
    }

    public async Task<AgentRequestResponse?> GetMyRequestAsync()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            _logger.LogWarning("Invalid JWT: userId claim is missing");
            return null;
        }

        var request = await _agentRequestRepository.GetByUserIdAsync(userId.Value);
        return request == null ? null : AgentRequestMapper.ToAgentRequestResponse(request);
    }

    public async Task<PagedResult<AgentRequestResponse>> GetAllAsync(PaginationRequest request, string? status = null)
    {
        var pagedResult = await _agentRequestRepository.GetAllAsync(request.PageNumber, request.PageSize, status);
        
        return new PagedResult<AgentRequestResponse>
        {
            Items = pagedResult.Items.Select(AgentRequestMapper.ToAgentRequestResponse).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<AgentRequestResponse> CreateAsync(CreateAgentRequestRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("Invalid JWT: userId claim is missing");
        }

        // Check if user already has a pending request
        var existingRequest = await _agentRequestRepository.GetByUserIdAsync(userId.Value);
        if (existingRequest != null && existingRequest.Status == "Pending")
        {
            throw new InvalidOperationException("You already have a pending agent request");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if user is already a sales agent
        if (user.Roles.Any(r => r.Name == "SalesAgent"))
        {
            throw new InvalidOperationException("User is already a sales agent");
        }

        var agentRequest = new AgentRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            RequestedAt = DateTime.UtcNow,
            Status = "Pending",
            Experience = request.Experience,
            Reason = request.Reason,
        };

        var created = await _agentRequestRepository.CreateAsync(agentRequest);
        _logger.LogInformation("Agent request created with ID {Id} for user {UserId}", created.Id, userId.Value);
        
        return AgentRequestMapper.ToAgentRequestResponse(created);
    }

    public async Task<ActivateUserResponse> ApproveAsync(Guid id)
    {
        var request = await _agentRequestRepository.GetByIdAsync(id);
        if (request == null)
        {
            return new ActivateUserResponse(false, "Agent request not found");
        }

        if (request.Status != "Pending")
        {
            return new ActivateUserResponse(false, $"Agent request is already {request.Status}");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return new ActivateUserResponse(false, "User not found");
        }

        // Check if user is already a sales agent
        if (user.Roles.Any(r => r.Name == "SalesAgent"))
        {
            request.Status = "Approved";
            request.ReviewedAt = DateTime.UtcNow;
            await _agentRequestRepository.UpdateAsync(request);
            return new ActivateUserResponse(false, "User is already a sales agent");
        }

        var salesAgentRole = await _roleRepository.GetByNameAsync("SalesAgent");
        if (salesAgentRole == null)
        {
            _logger.LogError("SalesAgent role not found in the database");
            return new ActivateUserResponse(false, "Internal error: SalesAgent role not found");
        }

        // Add SalesAgent role to user
        user.Roles.Add(salesAgentRole);
        await _userRepository.UpdateAsync(user);

        // Update request status
        request.Status = "Approved";
        request.ReviewedBy = _currentUserService.UserId;
        request.ReviewedAt = DateTime.UtcNow;
        await _agentRequestRepository.UpdateAsync(request);

        _logger.LogInformation("Agent request {Id} approved. User {UserId} is now a SalesAgent", id, request.UserId);
        return new ActivateUserResponse(true, "Agent request approved successfully. User is now a SalesAgent");
    }

    public async Task<ActivateUserResponse> RejectAsync(Guid id, string? reason = null)
    {
        var request = await _agentRequestRepository.GetByIdAsync(id);
        if (request == null)
        {
            return new ActivateUserResponse(false, "Agent request not found");
        }

        if (request.Status != "Pending")
        {
            return new ActivateUserResponse(false, $"Agent request is already {request.Status}");
        }

        request.Status = "Rejected";
        request.ReviewedBy = _currentUserService.UserId;
        request.ReviewedAt = DateTime.UtcNow;
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            request.Notes = string.IsNullOrWhiteSpace(request.Notes) 
                ? $"Rejection reason: {reason}" 
                : $"{request.Notes}\n\nRejection reason: {reason}";
        }

        await _agentRequestRepository.UpdateAsync(request);

        _logger.LogInformation("Agent request {Id} rejected for user {UserId}", id, request.UserId);
        return new ActivateUserResponse(true, "Agent request rejected successfully");
    }
}
