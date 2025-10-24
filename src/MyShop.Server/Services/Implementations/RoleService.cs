namespace MyShop.Server.Services.Interfaces;

using MyShop.Data.Repositories.Interfaces;
using MyShop.Shared.DTOs.Responses;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

        private readonly ILogger<RoleService> _logger;

    public RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<RoleResponse>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            
            return roles.Select(r => new RoleResponse
            {
                Name = r.Name,
                Description = r.Description
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            throw;
        }
    }

    public async Task<RoleResponse?> GetRoleByNameAsync(string roleName)
    {
        try
        {
            var role = await _roleRepository.GetByNameAsync(roleName);
            if (role == null) return null;

            return new RoleResponse
            {
                Name = role.Name,
                Description = role.Description
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by name: {RoleName}", roleName);
            throw;
        }
    }

}