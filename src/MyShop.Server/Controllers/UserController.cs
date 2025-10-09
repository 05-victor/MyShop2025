using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Shared.DTOs;
using MyShop.Data.Entities;
using BCrypt.Net;

namespace MyShop.Server.Controllers
{
    /// <summary>
    /// Controller xử lý các hoạt động quản lý người dùng.
    /// Cung cấp các API endpoint để tạo, cập nhật và quản lý người dùng.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ShopContext _context;

        /// <summary>
        /// Khởi tạo UserController với database context.
        /// </summary>
        /// <param name="context">Database context</param>
        public UserController(ShopContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo người dùng mới với thông tin đầy đủ.
        /// </summary>
        /// <param name="request">Thông tin người dùng mới</param>
        /// <returns>Thông tin người dùng đã tạo</returns>
        [HttpPost("create")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
        {
            // Validate đầu vào
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Sdt))
            {
                return BadRequest("Tất cả các trường đều bắt buộc.");
            }

            // Kiểm tra xem username hoặc email đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                return BadRequest("Username hoặc email đã tồn tại.");
            }

            // Mã hóa mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo người dùng mới
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword,
                Sdt = request.Sdt,
                CreatedAt = DateTime.UtcNow,
                ActivateTrial = request.ActivateTrial,
                Avatar = request.Avatar ?? string.Empty
            };

            // Thêm roles nếu có
            if (request.RoleNames?.Any() == true)
            {
                var roles = await _context.Roles
                    .Where(r => request.RoleNames.Contains(r.Name))
                    .ToListAsync();
                
                foreach (var role in roles)
                {
                    user.Roles.Add(role);
                }
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Load lại user với roles để trả về
            var createdUser = await _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Authorities)
                .FirstAsync(u => u.Id == user.Id);

            return Ok(MapUserToDto(createdUser));
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng.
        /// </summary>
        /// <returns>Danh sách người dùng</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Authorities)
                .ToListAsync();

            return Ok(users.Select(MapUserToDto));
        }

        /// <summary>
        /// Lấy thông tin người dùng theo ID.
        /// </summary>
        /// <param name="id">ID của người dùng</param>
        /// <returns>Thông tin người dùng</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Authorities)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            return Ok(MapUserToDto(user));
        }

        /// <summary>
        /// Chuyển đổi User entity thành UserDto.
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>UserDto</returns>
        private static UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Sdt = user.Sdt,
                CreatedAt = user.CreatedAt,
                ActivateTrial = user.ActivateTrial,
                Avatar = user.Avatar,
                Roles = user.Roles?.Select(r => new RoleDto
                {
                    Name = r.Name,
                    Description = r.Description,
                    Authorities = r.Authorities?.Select(a => new AuthorityDto
                    {
                        Name = a.Name,
                        Description = a.Description
                    }).ToList() ?? new List<AuthorityDto>()
                }).ToList() ?? new List<RoleDto>()
            };
        }
    }
}