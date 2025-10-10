/*
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Shared.DTOs;
using MyShop.Data.Entities;

namespace MyShop.Server.Controllers
{
    /// <summary>
    /// Controller xử lý các hoạt động quản lý vai trò.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly ShopContext _context;

        public RoleController(ShopContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo vai trò mới.
        /// </summary>
        /// <param name="request">Thông tin vai trò mới</param>
        /// <returns>Thông tin vai trò đã tạo</returns>
        [HttpPost("create")]
        public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Tên vai trò không được rỗng.");
            }

            // Kiểm tra xem role đã tồn tại chưa
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.Name);

            if (existingRole != null)
            {
                return BadRequest("Vai trò đã tồn tại.");
            }

            // Tạo role mới
            var role = new Role
            {
                Name = request.Name,
                Description = request.Description
            };

            // Thêm authorities nếu có
            if (request.AuthorityNames?.Any() == true)
            {
                var authorities = await _context.Authorities
                    .Where(a => request.AuthorityNames.Contains(a.Name))
                    .ToListAsync();

                foreach (var authority in authorities)
                {
                    role.Authorities.Add(authority);
                }
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Load lại role với authorities
            var createdRole = await _context.Roles
                .Include(r => r.Authorities)
                .FirstAsync(r => r.Name == role.Name);

            return Ok(MapRoleToDto(createdRole));
        }

        /// <summary>
        /// Lấy danh sách tất cả vai trò.
        /// </summary>
        /// <returns>Danh sách vai trò</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _context.Roles
                .Include(r => r.Authorities)
                .ToListAsync();

            return Ok(roles.Select(MapRoleToDto));
        }

        /// <summary>
        /// Lấy thông tin vai trò theo tên.
        /// </summary>
        /// <param name="name">Tên vai trò</param>
        /// <returns>Thông tin vai trò</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<RoleDto>> GetRole(string name)
        {
            var role = await _context.Roles
                .Include(r => r.Authorities)
                .FirstOrDefaultAsync(r => r.Name == name);

            if (role == null)
            {
                return NotFound("Không tìm thấy vai trò.");
            }

            return Ok(MapRoleToDto(role));
        }

        /// <summary>
        /// Chuyển đổi Role entity thành RoleDto.
        /// </summary>
        /// <param name="role">Role entity</param>
        /// <returns>RoleDto</returns>
        private static RoleDto MapRoleToDto(Role role)
        {
            return new RoleDto
            {
                Name = role.Name,
                Description = role.Description,
                Authorities = role.Authorities?.Select(a => new AuthorityDto
                {
                    Name = a.Name,
                    Description = a.Description
                }).ToList() ?? new List<AuthorityDto>()
            };
        }
    }
}
*/