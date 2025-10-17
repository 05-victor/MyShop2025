using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Data.Entities;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service implementation cho quản lý quyền hạn của user.
/// </summary>
public class UserAuthorityService : IUserAuthorityService
{
    private readonly ShopContext _context;
    private readonly ILogger<UserAuthorityService> _logger;

    public UserAuthorityService(ShopContext context, ILogger<UserAuthorityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetEffectiveAuthoritiesAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.RoleAuthorities)
                    .ThenInclude(ra => ra.Authority)
            .Include(u => u.RemovedAuthorities)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return Enumerable.Empty<string>();
        }

        // Get all authorities from roles
        var roleAuthorities = user.Roles
            .SelectMany(r => r.RoleAuthorities)
            .Select(ra => ra.AuthorityName)
            .Distinct()
            .ToList();

        // Get removed authorities
        var removedAuthorities = user.RemovedAuthorities
            .Select(ra => ra.AuthorityName)
            .ToHashSet();

        // Calculate effective authorities
        var effectiveAuthorities = roleAuthorities
            .Where(auth => !removedAuthorities.Contains(auth))
            .ToList();

        _logger.LogInformation(
            "User {UserId} has {RoleAuthCount} authorities from roles, {RemovedCount} removed, {EffectiveCount} effective",
            userId, roleAuthorities.Count, removedAuthorities.Count, effectiveAuthorities.Count);

        return effectiveAuthorities;
    }

    public async Task<EffectiveAuthoritiesResponse?> GetEffectiveAuthoritiesDetailAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
                .ThenInclude(r => r.RoleAuthorities)
                    .ThenInclude(ra => ra.Authority)
            .Include(u => u.RemovedAuthorities)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return null;
        }

        // Get all authorities from roles
        var roleAuthorities = user.Roles
            .SelectMany(r => r.RoleAuthorities)
            .Select(ra => ra.AuthorityName)
            .Distinct()
            .ToList();

        // Get removed authorities
        var removedAuthorities = user.RemovedAuthorities
            .Select(ra => ra.AuthorityName)
            .ToList();

        // Calculate effective authorities
        var effectiveAuthorities = roleAuthorities
            .Where(auth => !removedAuthorities.Contains(auth))
            .ToList();

        return new EffectiveAuthoritiesResponse
        {
            UserId = user.Id,
            Username = user.Username,
            RoleNames = user.Roles.Select(r => r.Name).ToList(),
            AllAuthoritiesFromRoles = roleAuthorities,
            RemovedAuthorities = removedAuthorities,
            EffectiveAuthorities = effectiveAuthorities
        };
    }

    public async Task<CheckAuthorityResponse> HasAuthorityAsync(Guid userId, string authorityName)
    {
        var effectiveAuthorities = await GetEffectiveAuthoritiesAsync(userId);
        var authList = effectiveAuthorities.ToList();

        // Check for "ALL" authority (super admin privilege)
        if (authList.Contains("ALL"))
        {
            return new CheckAuthorityResponse
            {
                UserId = userId,
                AuthorityName = authorityName,
                HasAuthority = true,
                Reason = "User has 'ALL' authority (super admin)"
            };
        }

        // Check for specific authority
        var hasAuthority = authList.Contains(authorityName);

        return new CheckAuthorityResponse
        {
            UserId = userId,
            AuthorityName = authorityName,
            HasAuthority = hasAuthority,
            Reason = hasAuthority 
                ? $"User has '{authorityName}' authority from their roles" 
                : $"User does not have '{authorityName}' authority (may be removed or not granted)"
        };
    }

    public async Task<RemovedAuthorityResponse> AddRemovedAuthorityAsync(Guid userId, AddRemovedAuthorityRequest request)
    {
        // Check if user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        // Check if authority exists
        var authorityExists = await _context.Authorities.AnyAsync(a => a.Name == request.AuthorityName);
        if (!authorityExists)
        {
            throw new InvalidOperationException($"Authority '{request.AuthorityName}' does not exist");
        }

        // Check if already removed
        var existingRemoval = await _context.RemovedAuthorities
            .FirstOrDefaultAsync(ra => ra.UserId == userId && ra.AuthorityName == request.AuthorityName);

        if (existingRemoval != null)
        {
            throw new InvalidOperationException($"Authority '{request.AuthorityName}' is already removed for this user");
        }

        // Add to removed authorities
        var removedAuthority = new RemovedAuthorities
        {
            UserId = userId,
            AuthorityName = request.AuthorityName,
            Reason = request.Reason,
            RemovedAt = DateTime.UtcNow,
            RemovedBy = request.RemovedBy
        };

        _context.RemovedAuthorities.Add(removedAuthority);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Removed authority '{Authority}' from user {UserId}. Reason: {Reason}",
            request.AuthorityName, userId, request.Reason ?? "Not specified");

        return new RemovedAuthorityResponse
        {
            UserId = removedAuthority.UserId,
            AuthorityName = removedAuthority.AuthorityName,
            Reason = removedAuthority.Reason,
            RemovedAt = removedAuthority.RemovedAt,
            RemovedBy = removedAuthority.RemovedBy
        };
    }

    public async Task<bool> RemoveRemovedAuthorityAsync(Guid userId, string authorityName)
    {
        var removedAuthority = await _context.RemovedAuthorities
            .FirstOrDefaultAsync(ra => ra.UserId == userId && ra.AuthorityName == authorityName);

        if (removedAuthority == null)
        {
            _logger.LogWarning(
                "Attempted to restore authority '{Authority}' for user {UserId}, but it was not in removed list",
                authorityName, userId);
            return false;
        }

        _context.RemovedAuthorities.Remove(removedAuthority);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Restored authority '{Authority}' for user {UserId}",
            authorityName, userId);

        return true;
    }

    public async Task<IEnumerable<RemovedAuthorityResponse>> GetRemovedAuthoritiesAsync(Guid userId)
    {
        var removedAuthorities = await _context.RemovedAuthorities
            .Where(ra => ra.UserId == userId)
            .Select(ra => new RemovedAuthorityResponse
            {
                UserId = ra.UserId,
                AuthorityName = ra.AuthorityName,
                Reason = ra.Reason,
                RemovedAt = ra.RemovedAt,
                RemovedBy = ra.RemovedBy
            })
            .ToListAsync();

        return removedAuthorities;
    }
}
