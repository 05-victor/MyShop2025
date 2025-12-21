using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.Enums;

namespace MyShop.Data.Repositories.Implementations;

public class AgentRequestRepository : IAgentRequestRepository
{
    private readonly ShopContext _context;
    private readonly ILogger<AgentRequestRepository> _logger;

    public AgentRequestRepository(ShopContext context, ILogger<AgentRequestRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AgentRequest?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.AgentRequests
                .Include(ar => ar.User)
                    .ThenInclude(u => u!.Profile)
                .FirstOrDefaultAsync(ar => ar.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent request by ID {Id}", id);
            throw;
        }
    }

    public async Task<AgentRequest?> GetByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.AgentRequests
                .Include(ar => ar.User)
                    .ThenInclude(u => u!.Profile)
                .Where(ar => ar.UserId == userId)
                .OrderByDescending(ar => ar.RequestedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent request by user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<PagedResult<AgentRequest>> GetAllAsync(int pageNumber = 1, int pageSize = 20, AgentRequestStatus? status = null)
    {
        try
        {
            var query = _context.AgentRequests
                .Include(ar => ar.User)
                    .ThenInclude(u => u!.Profile)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(ar => ar.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(ar => ar.RequestedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AgentRequest>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged agent requests");
            throw;
        }
    }

    public async Task<AgentRequest> CreateAsync(AgentRequest agentRequest)
    {
        try
        {
            _context.AgentRequests.Add(agentRequest);
            await _context.SaveChangesAsync();
            
            // Load navigation properties
            await _context.Entry(agentRequest)
                .Reference(ar => ar.User)
                .LoadAsync();
                
            if (agentRequest.User != null)
            {
                await _context.Entry(agentRequest.User)
                    .Reference(u => u.Profile)
                    .LoadAsync();
            }
            
            return agentRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent request");
            throw;
        }
    }

    public async Task<AgentRequest> UpdateAsync(AgentRequest agentRequest)
    {
        try
        {
            _context.AgentRequests.Update(agentRequest);
            await _context.SaveChangesAsync();
            
            // Reload navigation properties
            await _context.Entry(agentRequest)
                .Reference(ar => ar.User)
                .LoadAsync();
                
            if (agentRequest.User != null)
            {
                await _context.Entry(agentRequest.User)
                    .Reference(u => u.Profile)
                    .LoadAsync();
            }
            
            return agentRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent request");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var agentRequest = await _context.AgentRequests.FindAsync(id);
            if (agentRequest == null)
            {
                return false;
            }

            _context.AgentRequests.Remove(agentRequest);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting agent request {Id}", id);
            throw;
        }
    }
}
