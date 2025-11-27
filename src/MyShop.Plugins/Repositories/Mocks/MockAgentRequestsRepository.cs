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

    public async Task<Result<IEnumerable<AgentRequest>>> GetAllAsync()
    {
        try
        {
            var mockData = await MockAgentRequestsData.GetAllAsync();
            
            var requests = mockData.Select(m => new AgentRequest
            {
                Id = Guid.Parse(m.Id),
                UserId = Guid.Parse(m.UserId),
                FullName = m.UserName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                AvatarUrl = m.AvatarUrl,
                RequestedAt = m.RequestedAt,
                Status = m.Status,
                ReviewedBy = m.ReviewedBy,
                ReviewedAt = m.ReviewedAt,
                Notes = m.Notes
            }).OrderByDescending(r => r.RequestedAt).ToList();

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetAllAsync returned {requests.Count} requests");
            return Result<IEnumerable<AgentRequest>>.Success(requests);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetAllAsync error: {ex.Message}");
            return Result<IEnumerable<AgentRequest>>.Failure($"Failed to get agent requests: {ex.Message}");
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

            var requests = mockData.Select(m => new AgentRequest
            {
                Id = Guid.Parse(m.Id),
                UserId = Guid.Parse(m.UserId),
                FullName = m.UserName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                AvatarUrl = m.AvatarUrl,
                RequestedAt = m.RequestedAt,
                Status = m.Status,
                ReviewedBy = m.ReviewedBy,
                ReviewedAt = m.ReviewedAt,
                Notes = m.Notes
            }).ToList();

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
            var result = await MockAgentRequestsData.CreateAsync(agentRequest);
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

    public async Task<Result<bool>> RejectAsync(Guid id)
    {
        try
        {
            var result = await MockAgentRequestsData.RejectAsync(id, Guid.Empty, "Rejected by admin");
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
}
