/*
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Shared.DTOs;
using MyShop.Data.Entities;

namespace MyShop.Server.Controllers
{
    /// <summary>
    /// Controller xử lý các hoạt động quản lý quyền hạn.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorityController : ControllerBase
    {
        private readonly ShopContext _context;

        public AuthorityController(ShopContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo quyền hạn mới.
        /// </summary>
        /// <param name="request">Thông tin quyền hạn mới</param>
        /// <returns>Thông tin quyền hạn đã tạo</returns>
        [HttpPost("create")]
        public async Task<ActionResult<AuthorityDto>> CreateAuthority(CreateAuthorityRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Tên quyền hạn không được rỗng.");
            }

            // Kiểm tra xem authority đã tồn tại chưa
            var existingAuthority = await _context.Authorities
                .FirstOrDefaultAsync(a => a.Name == request.Name);

            if (existingAuthority != null)
            {
                return BadRequest("Quyền hạn đã tồn tại.");
            }

            // Tạo authority mới
            var authority = new Authority
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.Authorities.Add(authority);
            await _context.SaveChangesAsync();

            return Ok(new AuthorityDto
            {
                Id = Guid.NewGuid(), // Tạm thời generate ID vì entity không có
                Name = authority.Name,
                Description = authority.Description,
                Module = request.Module,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        /// <summary>
        /// Lấy danh sách tất cả quyền hạn.
        /// </summary>
        /// <returns>Danh sách quyền hạn</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorityDto>>> GetAllAuthorities()
        {
            var authorities = await _context.Authorities.ToListAsync();

            return Ok(authorities.Select(a => new AuthorityDto
            {
                Id = Guid.NewGuid(), // Tạm thời generate ID vì entity không có
                Name = a.Name,
                Description = a.Description,
                Module = null, // Entity không có Module field
                CreatedAt = DateTime.UtcNow, // Entity không có CreatedAt field
                IsActive = true // Entity không có IsActive field
            }));
        }

        /// <summary>
        /// Lấy thông tin quyền hạn theo tên.
        /// </summary>
        /// <param name="name">Tên quyền hạn</param>
        /// <returns>Thông tin quyền hạn</returns>
        [HttpGet("{name}")]
        public async Task<ActionResult<AuthorityDto>> GetAuthority(string name)
        {
            var authority = await _context.Authorities
                .FirstOrDefaultAsync(a => a.Name == name);

            if (authority == null)
            {
                return NotFound("Không tìm thấy quyền hạn.");
            }

            return Ok(new AuthorityDto
            {
                Id = Guid.NewGuid(), // Tạm thời generate ID vì entity không có
                Name = authority.Name,
                Description = authority.Description,
                Module = null, // Entity không có Module field
                CreatedAt = DateTime.UtcNow, // Entity không có CreatedAt field
                IsActive = true // Entity không có IsActive field
            });
        }
    }
}
*/