using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Repositories.Mocks.Data;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for agent requests repository using in-memory storage
/// </summary>
public class MockAgentRequestsRepository : IAgentRequestRepository
{
    // In-memory cache of agent requests
    private static List<AgentRequest>? _requests;
    private static readonly object _lock = new object();

    public MockAgentRequestsRepository()
    {
        EnsureDataLoaded();
    }

    private static void EnsureDataLoaded()
    {
        if (_requests != null) return;

        lock (_lock)
        {
            if (_requests != null) return;

            // Load from JSON data provider
            var mockData = MockAgentRequestsData.GetAllAsync().GetAwaiter().GetResult();
            
            _requests = mockData.Select(m => new AgentRequest
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

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] Loaded {_requests.Count} agent requests into cache");
        }
    }

    public Task<IEnumerable<AgentRequest>> GetAllAsync()
    {
        EnsureDataLoaded();
        
        // Return ordered by RequestedAt descending (newest first)
        var result = _requests!
            .OrderByDescending(r => r.RequestedAt)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] GetAllAsync returned {result.Count} requests");
        return Task.FromResult<IEnumerable<AgentRequest>>(result);
    }

    public Task<bool> ApproveAsync(Guid id)
    {
        EnsureDataLoaded();

        lock (_lock)
        {
            var request = _requests!.FirstOrDefault(r => r.Id == id);
            
            if (request == null)
            {
                System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] ApproveAsync - Request {id} not found");
                return Task.FromResult(false);
            }

            request.Status = "Approved";
            request.ReviewedBy = "Nguyễn Quản Trị";
            request.ReviewedAt = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] Approved request {id} for {request.FullName}");
            return Task.FromResult(true);
        }
    }

    public Task<bool> RejectAsync(Guid id)
    {
        EnsureDataLoaded();

        lock (_lock)
        {
            var request = _requests!.FirstOrDefault(r => r.Id == id);
            
            if (request == null)
            {
                System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] RejectAsync - Request {id} not found");
                return Task.FromResult(false);
            }

            request.Status = "Rejected";
            request.ReviewedBy = "Nguyễn Quản Trị";
            request.ReviewedAt = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"[MockAgentRequestsRepository] Rejected request {id} for {request.FullName}");
            return Task.FromResult(true);
        }
    }
}
