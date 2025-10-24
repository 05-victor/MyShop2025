using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetAllRolesAsync();

    Task<RoleResponse?> GetRoleByNameAsync(string roleName);
}
