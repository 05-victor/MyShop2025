using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Users;
using MyShop.Shared.Models;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Common;
using Refit;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based Agent Request Repository implementation
/// Calls backend via IAgentRequestsApi (Refit)
/// </summary>
public class AgentRequestRepository : IAgentRequestRepository
{
    private readonly IAgentRequestsApi _api;

    public AgentRequestRepository(IAgentRequestsApi api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public async Task<Result<IEnumerable<AgentRequest>>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: 1000);
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetAllAsync] API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse?.Result != null)
                {
                    var pagedResult = apiResponse.Result;
                    System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetAllAsync] PagedResult - Items: {pagedResult.Items.Count}, TotalCount: {pagedResult.TotalCount}");

                    var requests = pagedResult.Items.Select(MapResponseToModel).ToList();
                    System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetAllAsync] Mapped {requests.Count} requests");
                    return Result<IEnumerable<AgentRequest>>.Success(requests);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetAllAsync] ❌ Failed - IsSuccess: {response.IsSuccessStatusCode}, Content: {(response.Content != null ? "NotNull" : "Null")}");
            return Result<IEnumerable<AgentRequest>>.Failure("Failed to retrieve agent requests");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetAllAsync] ❌ API Error: {ex.StatusCode} - {ex.Message}");
            return Result<IEnumerable<AgentRequest>>.Failure($"API error retrieving requests: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetAllAsync] ❌ Unexpected Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return Result<IEnumerable<AgentRequest>>.Failure($"Error retrieving requests: {ex.Message}");
        }
    }

    public async Task<Result<AgentRequest>> GetByIdAsync(Guid id)
    {
        try
        {
            var response = await _api.GetByIdAsync(id);
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse?.Result != null)
                {
                    var request = MapResponseToModel(apiResponse.Result);
                    System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByIdAsync] Successfully retrieved request {id}");
                    return Result<AgentRequest>.Success(request);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByIdAsync] ❌ Request {id} not found");
            return Result<AgentRequest>.Failure($"Agent request with ID {id} not found");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByIdAsync] ❌ API Error: {ex.StatusCode} - {ex.Message}");
            return Result<AgentRequest>.Failure($"API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByIdAsync] ❌ Unexpected Error: {ex.Message}");
            return Result<AgentRequest>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PagedList<AgentRequest>>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "requestedAt",
        bool sortDescending = true)
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber: page, pageSize: pageSize, status: status);
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetPagedAsync] API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse?.Result != null)
                {
                    var pagedResult = apiResponse.Result;
                    var requests = pagedResult.Items.Select(MapResponseToModel).ToList();

                    var pagedList = new PagedList<AgentRequest>
                    {
                        Items = requests,
                        TotalCount = pagedResult.TotalCount,
                        PageNumber = pagedResult.Page,
                        PageSize = pagedResult.PageSize
                    };

                    System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetPagedAsync] Retrieved {requests.Count} requests");
                    return Result<PagedList<AgentRequest>>.Success(pagedList);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetPagedAsync] ❌ Failed to retrieve paged requests");
            return Result<PagedList<AgentRequest>>.Failure("Failed to retrieve agent requests");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetPagedAsync] ❌ API Error: {ex.StatusCode} - {ex.Message}");
            return Result<PagedList<AgentRequest>>.Failure($"API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetPagedAsync] ❌ Unexpected Error: {ex.Message}");
            return Result<PagedList<AgentRequest>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<AgentRequest>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var response = await _api.GetMyRequestAsync();
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByUserIdAsync] API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse?.Result != null)
                {
                    var request = MapResponseToModel(apiResponse.Result);
                    System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByUserIdAsync] Retrieved request for user {userId}");
                    return Result<AgentRequest>.Success(request);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByUserIdAsync] ❌ No request found for user {userId}");
            return Result<AgentRequest>.Failure($"No agent request found for user {userId}");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByUserIdAsync] ❌ API Exception: {ex.Message}");
            return Result<AgentRequest>.Failure($"API Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetByUserIdAsync] ❌ Exception: {ex.Message}");
            return Result<AgentRequest>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>> GetMyRequestAsync()
    {
        try
        {
            var response = await _api.GetMyRequestAsync();
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetMyRequestAsync] API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse?.Result != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetMyRequestAsync] Retrieved request");
                    return Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>.Success(apiResponse.Result);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetMyRequestAsync] No request found");
            return Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>.Success(null);
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetMyRequestAsync] ❌ API Error: {ex.StatusCode} - {ex.Message}");
            return Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>.Failure($"API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.GetMyRequestAsync] ❌ Unexpected Error: {ex.Message}");
            return Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<AgentRequest>> CreateAsync(AgentRequest agentRequest)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.CreateAsync] Creating agent request");

            var request = new MyShop.Shared.DTOs.Requests.CreateAgentRequestRequest
            {
                Experience = agentRequest.Experience,
                Reason = agentRequest.Reason
            };

            var response = await _api.CreateAsync(request);
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.CreateAsync] API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse?.Result != null)
                {
                    var createdRequest = MapResponseToModel(apiResponse.Result);
                    System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.CreateAsync] ✓ Agent request created: {createdRequest.Id}");
                    return Result<AgentRequest>.Success(createdRequest);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.CreateAsync] ❌ Failed to create agent request");
            return Result<AgentRequest>.Failure("Failed to create agent request");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.CreateAsync] ❌ API Error: {ex.StatusCode} - {ex.Message}");
            return Result<AgentRequest>.Failure($"API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.CreateAsync] ❌ Unexpected Error: {ex.Message}");
            return Result<AgentRequest>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ApproveAsync(Guid id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.ApproveAsync] Approving request {id}");

            var response = await _api.ApproveAsync(id);
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.ApproveAsync] API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.ApproveAsync] ✓ Request approved: {id}");
                return Result<bool>.Success(true);
            }

            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.ApproveAsync] ❌ Failed to approve request");
            return Result<bool>.Failure("Failed to approve request");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.ApproveAsync] ❌ API Error: {ex.StatusCode} - {ex.Message}");
            return Result<bool>.Failure($"API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.ApproveAsync] ❌ Unexpected Error: {ex.Message}");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RejectAsync(Guid id, string reason = "")
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] Rejecting request {id}");
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] Reason received: '{reason}'");
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] Reason is null: {reason == null}");
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] Reason is empty: {string.IsNullOrWhiteSpace(reason)}");

            // Use proper DTO that matches server's RejectAgentRequest
            var rejectRequest = new RejectAgentRequest { Reason = reason };
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] Sending to API: {System.Text.Json.JsonSerializer.Serialize(rejectRequest)}");

            var response = await _api.RejectAsync(id, rejectRequest);
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] ✓ Request rejected: {id}");
                return Result<bool>.Success(true);
            }

            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] ❌ Failed to reject request");
            return Result<bool>.Failure("Failed to reject request");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] ❌ API Error: {ex.StatusCode} - {ex.Message}");
            return Result<bool>.Failure($"API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AgentRequestRepository.RejectAsync] ❌ Unexpected Error: {ex.Message}");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Maps AgentRequestResponse DTO to AgentRequest model
    /// </summary>
    private static AgentRequest MapResponseToModel(AgentRequestResponse response)
    {
        return new AgentRequest
        {
            Id = response.Id,
            UserId = response.UserId,
            FullName = response.FullName,
            Email = response.Email,
            PhoneNumber = response.PhoneNumber,
            AvatarUrl = response.AvatarUrl ?? string.Empty,
            RequestedAt = response.RequestedAt,
            Status = response.Status,
            ReviewedBy = response.ReviewedBy,
            ReviewedAt = response.ReviewedAt,
            Notes = response.Notes ?? string.Empty,
            Experience = response.Experience,
            Reason = response.Reason,
            Address = response.Address ?? string.Empty
        };
    }
}
