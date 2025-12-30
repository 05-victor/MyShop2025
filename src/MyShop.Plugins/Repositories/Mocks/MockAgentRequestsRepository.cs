using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Repositories.Mocks.Data;
using MyShop.Shared.Models;
using MyShop.Core.Common;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for agent requests repository - delegates to MockAgentRequestsData
/// </summary>
public class MockAgentRequestsRepository : IAgentRequestRepository
{
    /// <summary>
    /// Batch load users by IDs to avoid N+1 query problem
    /// </summary>
    private static async Task<Dictionary<Guid, User>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        var result = new Dictionary<Guid, User>();
        if (userIds.Count == 0) return result;

        // Load all users once (no network delay per item)
        var allUsers = await MyShop.Plugins.Mocks.Data.MockUserData.GetAllAsync();

        foreach (var user in allUsers)
        {
            if (userIds.Contains(user.Id))
            {
                result[user.Id] = user;
            }
        }

        return result;
    }

    public async Task<Result<IEnumerable<AgentRequest>>> GetAllAsync()
    {
        try
        {
            var mockData = await MockAgentRequestsData.GetAllAsync();

            // Batch load all users to avoid N+1 query problem
            var userIds = mockData.Select(m => Guid.Parse(m.UserId)).Distinct().ToList();
            var usersDict = await GetUsersByIdsAsync(userIds);

            var requests = new List<AgentRequest>();
            foreach (var m in mockData)
            {
                var agent = new AgentRequest
                {
                    Id = Guid.Parse(m.Id),
                    UserId = Guid.Parse(m.UserId),
                    RequestedAt = m.RequestedAt,
                    Status = m.Status,
                    ReviewedBy = m.ReviewedBy,
                    ReviewedAt = m.ReviewedAt,
                    Notes = m.Notes
                };

                // Enrich with user profile from cached users
                if (usersDict.TryGetValue(agent.UserId, out var user))
                {
                    agent.FullName = user.FullName ?? user.Username;
                    agent.Email = user.Email;
                    agent.PhoneNumber = user.PhoneNumber ?? string.Empty;
                    agent.AvatarUrl = user.Avatar ?? string.Empty;
                }

                requests.Add(agent);
            }

            requests = requests.OrderByDescending(r => r.RequestedAt).ToList();

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetAllAsync returned {requests.Count} requests");
            return Result<IEnumerable<AgentRequest>>.Success(requests);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetAllAsync error: {ex.Message}");
            return Result<IEnumerable<AgentRequest>>.Failure($"Failed to get agent requests: {ex.Message}");
        }
    }

    public async Task<Result<AgentRequest>> GetByIdAsync(Guid id)
    {
        try
        {
            var allResult = await GetAllAsync();
            if (!allResult.IsSuccess || allResult.Data == null)
            {
                return Result<AgentRequest>.Failure("Failed to load agent requests");
            }

            var request = allResult.Data.FirstOrDefault(r => r.Id == id);
            if (request == null)
            {
                return Result<AgentRequest>.Failure($"Agent request {id} not found");
            }

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetByIdAsync returned request {id}");
            return Result<AgentRequest>.Success(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetByIdAsync error: {ex.Message}");
            return Result<AgentRequest>.Failure($"Failed to get agent request: {ex.Message}");
        }
    }

    public async Task<Result<AgentRequest>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var allResult = await GetAllAsync();
            if (!allResult.IsSuccess || allResult.Data == null)
            {
                return Result<AgentRequest>.Failure("Failed to load agent requests");
            }

            var request = allResult.Data
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .FirstOrDefault();

            if (request == null)
            {
                return Result<AgentRequest>.Failure($"No agent request found for user {userId}");
            }

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetByUserIdAsync returned request for user {userId}");
            return Result<AgentRequest>.Success(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetByUserIdAsync error: {ex.Message}");
            return Result<AgentRequest>.Failure($"Failed to get agent request: {ex.Message}");
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
        try
        {
            var (mockData, totalCount) = await MockAgentRequestsData.GetPagedAsync(
                page, pageSize, status, searchQuery, sortBy, sortDescending);

            // Batch load all users to avoid N+1 query problem
            var userIds = mockData.Select(m => Guid.Parse(m.UserId)).Distinct().ToList();
            var usersDict = await GetUsersByIdsAsync(userIds);

            var requests = new List<AgentRequest>();
            foreach (var m in mockData)
            {
                var agent = new AgentRequest
                {
                    Id = Guid.Parse(m.Id),
                    UserId = Guid.Parse(m.UserId),
                    RequestedAt = m.RequestedAt,
                    Status = m.Status,
                    ReviewedBy = m.ReviewedBy,
                    ReviewedAt = m.ReviewedAt,
                    Notes = m.Notes
                };

                // Enrich with user profile from cached users
                if (usersDict.TryGetValue(agent.UserId, out var user))
                {
                    agent.FullName = user.FullName ?? user.Username;
                    agent.Email = user.Email;
                    agent.PhoneNumber = user.PhoneNumber ?? string.Empty;
                    agent.AvatarUrl = user.Avatar ?? string.Empty;
                }

                requests.Add(agent);
            }

            var pagedList = new PagedList<AgentRequest>(requests, totalCount, page, pageSize);

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetPagedAsync returned page {page}/{pagedList.TotalPages} ({requests.Count} items, {totalCount} total)");
            return Result<PagedList<AgentRequest>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetPagedAsync error: {ex.Message}");
            return Result<PagedList<AgentRequest>>.Failure($"Failed to get paged agent requests: {ex.Message}");
        }
    }

    public async Task<Result<AgentRequest>> CreateAsync(AgentRequest agentRequest)
    {
        try
        {
            // Ensure we do not duplicate user info; only store request linked to UserId
            var normalizedRequest = new AgentRequest
            {
                Id = agentRequest.Id != Guid.Empty ? agentRequest.Id : Guid.NewGuid(),
                UserId = agentRequest.UserId,
                RequestedAt = DateTime.UtcNow,
                Status = string.IsNullOrWhiteSpace(agentRequest.Status) ? "Pending" : agentRequest.Status,
                ReviewedBy = null,
                ReviewedAt = null,
                Notes = string.IsNullOrWhiteSpace(agentRequest.Notes) ?
                    $"Experience: {agentRequest.Experience}\n\nMotivation: {agentRequest.Reason}" : agentRequest.Notes
            };

            var result = await MockAgentRequestsData.CreateAsync(normalizedRequest);
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] CreateAsync result: {result != null}");
            return result != null
                ? Result<AgentRequest>.Success(result)
                : Result<AgentRequest>.Failure("Failed to create agent request");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] CreateAsync error: {ex.Message}");
            return Result<AgentRequest>.Failure($"Failed to create request: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ApproveAsync(Guid id)
    {
        try
        {
            var result = await MockAgentRequestsData.ApproveAsync(id, Guid.Empty);
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] ApproveAsync result: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Failed to approve agent request {id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] ApproveAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to approve request: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RejectAsync(Guid id, string reason = "")
    {
        try
        {
            var result = await MockAgentRequestsData.RejectAsync(id, Guid.Empty, reason ?? "Rejected by admin");
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] RejectAsync result: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Failed to reject agent request {id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] RejectAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to reject request: {ex.Message}");
        }
    }

    public async Task<Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>> GetMyRequestAsync()
    {
        try
        {
            // Mock implementation - return null (no request found)
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetMyRequestAsync - Mock returns null");
            return Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>.Success(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetMyRequestAsync error: {ex.Message}");
            return Result<MyShop.Shared.DTOs.Responses.AgentRequestResponse?>.Failure($"Failed to get my request: {ex.Message}");
        }
    }
}
