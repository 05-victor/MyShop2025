using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Shared.DTOs;
using MyShop.Data.Entities;
using BCrypt.Net;

namespace MyShop.Server.Controllers
{
    /// <summary>
    /// Controller xử lý các hoạt động xác thực người dùng.
    /// Cung cấp các API endpoint để đăng ký, đăng nhập và quản lý người dùng.
    /// </summary>
    /// <remarks>
    /// Controller này cung cấp các API sau:
    /// - POST /api/auth/register: Đăng ký tài khoản mới
    /// - POST /api/auth/login: Đăng nhập với email/username và mật khẩu  
    /// - GET /api/auth/me: Lấy thông tin người dùng hiện tại
    /// 
    /// Tất cả mật khẩu được mã hóa bằng BCrypt trước khi lưu vào database.
    /// Hiện tại sử dụng token đơn giản, trong tương lai nên chuyển sang JWT.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Database context để truy cập dữ liệu người dùng.
        /// </summary>
        private readonly ShopContext _context;
        
        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="AuthController"/>.
        /// </summary>  
        /// <param name="context">Database context để truy cập dữ liệu</param>
        public AuthController(ShopContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Đăng ký tài khoản người dùng mới.
        /// </summary>
        /// <param name="request">Thông tin đăng ký bao gồm username, email, password và sdt</param>
        /// <returns>
        /// ActionResult chứa AuthResponse với thông tin kết quả đăng ký.
        /// Trả về 200 OK nếu thành công, 400 BadRequest nếu có lỗi validation hoặc trùng lặp.
        /// </returns>
        /// <remarks>
        /// API này thực hiện các bước sau:
        /// 1. Validate thông tin đầu vào (username, email, password, sdt không được rỗng)
        /// 2. Kiểm tra xem email hoặc username đã tồn tại chưa
        /// 3. Mã hóa mật khẩu bằng BCrypt
        /// 4. Tạo và lưu người dùng mới vào database với UUID
        /// 5. Trả về thông tin người dùng và token đăng nhập
        /// </remarks>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            // Validate đầu vào
            if (string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Sdt))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false,
                    Message = "Tất cả các trường đều bắt buộc."
                });
            }

            // Kiểm tra xem email hoặc username đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Username);
            
            if (existingUser != null)
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false,
                    Message = "Email hoặc username đã tồn tại."
                });
            }

            // Mã hóa mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo người dùng mới với UUID
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword,
                Sdt = request.Sdt,
                CreatedAt = DateTime.UtcNow,
                ActivateTrial = false,
                Avatar = string.Empty
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Tạo token đơn giản (trong tương lai nên dùng JWT)
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{newUser.Id}:{DateTime.UtcNow.Ticks}"));

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Đăng ký thành công.",
                Token = token,
                User = new UserDto
                {
                    Id = newUser.Id,
                    Username = newUser.Username,
                    Email = newUser.Email,
                    Sdt = newUser.Sdt,
                    CreatedAt = newUser.CreatedAt,
                    ActivateTrial = newUser.ActivateTrial,
                    Avatar = newUser.Avatar
                }
            });
        }

        /// <summary>
        /// Đăng nhập người dùng.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập bao gồm username/email và password</param>
        /// <returns>
        /// ActionResult chứa AuthResponse với thông tin kết quả đăng nhập.
        /// Trả về 200 OK nếu thành công, 401 Unauthorized nếu thông tin không đúng.
        /// </returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            // Validate đầu vào
            if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false,
                    Message = "Username/Email và mật khẩu đều bắt buộc."
                });
            }

            // Tìm user theo username hoặc email
            var user = await _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Authorities)
                .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(new AuthResponse 
                { 
                    Success = false,
                    Message = "Thông tin đăng nhập không đúng."
                });
            }

            // Tạo token đơn giản (trong tương lai nên dùng JWT)
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Id}:{DateTime.UtcNow.Ticks}"));

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Đăng nhập thành công.",
                Token = token,
                User = new UserDto
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
                }
            });
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại.
        /// </summary>
        /// <returns>
        /// ActionResult chứa UserDto với thông tin người dùng.
        /// Trả về 200 OK nếu tìm thấy, 404 NotFound nếu không có người dùng nào.
        /// </returns>
        /// <remarks>
        /// API này hiện tại chỉ trả về người dùng đầu tiên trong database để test.
        /// Trong tương lai cần:
        /// 1. Xác thực token từ request header
        /// 2. Lấy user ID từ token đã decode
        /// 3. Trả về thông tin người dùng tương ứng với token
        /// </remarks>
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            // Tạm thời trả về user đầu tiên để test
            var user = await _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Authorities)
                .FirstOrDefaultAsync();
            
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng");
            }

            return Ok(new UserDto
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
            });
        }
    }
}