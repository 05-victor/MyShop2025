using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;

namespace MyShop.Client.Facades.Users;

/// <summary>
/// Facade for agent request operations
/// Aggregates: IAgentRequestRepository, IToastService
/// </summary>
public class AgentRequestFacade : IAgentRequestFacade
{
    private readonly IAgentRequestRepository _agentRequestRepository;
    private readonly IToastService _toastService;

    public AgentRequestFacade(IAgentRequestRepository agentRequestRepository, IToastService toastService)
    {
        _agentRequestRepository = agentRequestRepository ?? throw new ArgumentNullException(nameof(agentRequestRepository));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    public async Task<Result<List<AgentRequest>>> LoadRequestsAsync()
    {
        try
        {
            var result = await _agentRequestRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                await _toastService.ShowError("Failed to load agent requests");
                return Result<List<AgentRequest>>.Failure(result.ErrorMessage ?? "Failed to load requests");
            }
            return Result<List<AgentRequest>>.Success(result.Data.ToList());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Error loading requests: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<List<AgentRequest>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PagedList<AgentRequest>>> LoadRequestsAsync(string? status = null, string? searchQuery = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var result = await _agentRequestRepository.GetPagedAsync(
                page: page,
                pageSize: pageSize,
                status: status,
                searchQuery: searchQuery,
                sortBy: "requestedAt",
                sortDescending: true);

            if (!result.IsSuccess || result.Data == null)
            {
                await _toastService.ShowError("Failed to load agent requests");
                return Result<PagedList<AgentRequest>>.Failure(result.ErrorMessage ?? "Failed to load requests");
            }

            return Result<PagedList<AgentRequest>>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Error loading paged requests: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<PagedList<AgentRequest>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PagedList<AgentRequest>>> GetPagedAsync(
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "requestedAt",
        bool sortDescending = true)
    {
        return await LoadRequestsAsync(status, searchQuery, page, pageSize);
    }

    public async Task<Result<AgentRequest>> GetRequestByIdAsync(Guid requestId)
    {
        try
        {
            // IAgentRequestRepository doesn't have GetByIdAsync, use GetAllAsync and filter
            var allResult = await _agentRequestRepository.GetAllAsync();
            if (!allResult.IsSuccess || allResult.Data == null)
            {
                await _toastService.ShowError("Failed to load agent requests");
                return Result<AgentRequest>.Failure("Failed to load requests");
            }

            var request = allResult.Data.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                await _toastService.ShowError("Agent request not found");
                return Result<AgentRequest>.Failure("Request not found");
            }

            return Result<AgentRequest>.Success(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Error getting request: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<AgentRequest>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<AgentRequest>> SubmitRequestAsync(
        string reason,
        string experience,
        string fullName,
        string email,
        string phoneNumber,
        string address,
        string? businessName = null,
        string? taxId = null)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(reason) || string.IsNullOrWhiteSpace(experience))
            {
                await _toastService.ShowError("Please fill all required fields");
                return Result<AgentRequest>.Failure("Missing required fields");
            }

            // Create new agent request
            var newRequest = new AgentRequest
            {
                Id = Guid.NewGuid(),
                RequestedAt = DateTime.UtcNow,
                Status = "PENDING",
                Reason = reason,
                Experience = experience,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Address = address,
                BusinessName = businessName,
                TaxId = taxId
            };

            var result = await _agentRequestRepository.CreateAsync(newRequest);
            
            if (result.IsSuccess && result.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Agent request submitted: {newRequest.Id}");
                return Result<AgentRequest>.Success(result.Data);
            }
            else
            {
                await _toastService.ShowError("Failed to submit agent request");
                return Result<AgentRequest>.Failure(result.ErrorMessage ?? "Failed to submit request");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Error submitting request: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<AgentRequest>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ApproveRequestAsync(Guid requestId, string? notes = null)
    {
        try
        {
            var result = await _agentRequestRepository.ApproveAsync(requestId);
            if (result.IsSuccess && result.Data)
            {
                await _toastService.ShowSuccess("Agent request approved successfully");
                return Result<Unit>.Success(Unit.Value);
            }
            else
            {
                await _toastService.ShowError("Failed to approve request");
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to approve request");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Error approving request: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> RejectRequestAsync(Guid requestId, string reason)
    {
        try
        {
            var result = await _agentRequestRepository.RejectAsync(requestId);
            if (result.IsSuccess && result.Data)
            {
                await _toastService.ShowSuccess($"Agent request rejected");
                return Result<Unit>.Success(Unit.Value);
            }
            else
            {
                await _toastService.ShowError("Failed to reject request");
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to reject request");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Error rejecting request: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetPendingRequestsCountAsync()
    {
        try
        {
            // IAgentRequestRepository doesn't have GetPendingCountAsync, use GetAllAsync and filter
            var allResult = await _agentRequestRepository.GetAllAsync();
            if (!allResult.IsSuccess || allResult.Data == null)
            {
                return Result<int>.Success(0);
            }

            var pendingCount = allResult.Data.Count(r => r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
            return Result<int>.Success(pendingCount);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Error getting pending count: {ex.Message}");
            return Result<int>.Success(0);
        }
    }

    public async Task<Result<User>> GetRequestUserProfileAsync(Guid requestId)
    {
        try
        {
            await _toastService.ShowInfo("User profile - Feature coming soon");
            return Result<User>.Failure("GetRequestUserProfile not implemented");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestFacade] Error getting user profile: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<User>.Failure($"Error: {ex.Message}");
        }
    }
}
