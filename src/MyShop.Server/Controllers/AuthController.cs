using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Shared;
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
        /// <param name="request">Thông tin đăng ký bao gồm username, email và password</param>
        /// <returns>
        /// ActionResult chứa AuthResponse với thông tin kết quả đăng ký.
        /// Trả về 200 OK nếu thành công, 400 BadRequest nếu có lỗi validation hoặc trùng lặp.
        /// </returns>
        /// <remarks>
        /// API này thực hiện các bước sau:
        /// 1. Validate thông tin đầu vào (username, email, password không được rỗng)
        /// 2. Kiểm tra xem email hoặc username đã tồn tại chưa
        /// 3. Mã hóa mật khẩu bằng BCrypt
        /// 4. Tạo và lưu người dùng mới vào database
        /// 5. Trả về thông tin người dùng và token đăng nhập
        /// </remarks>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            // Validate đầu vào
            if (string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Vui lòng điền đầy đủ thông tin" 
                });
            }

            // Kiểm tra xem người dùng đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Username);
            
            if (existingUser != null)
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Email hoặc tên đăng nhập đã tồn tại" 
                });
            }

            // Mã hóa mật khẩu
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo người dùng mới
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Đăng ký thành công",
                Token = "temp_token_" + user.Id, // Tạm thời dùng token đơn giản
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                }
            });
        }

        /// <summary>
        /// Đăng nhập người dùng với email/username và mật khẩu.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập bao gồm email/username và password</param>
        /// <returns>
        /// ActionResult chứa AuthResponse với thông tin kết quả đăng nhập.
        /// Trả về 200 OK nếu thành công, 400 BadRequest nếu thông tin không đúng.
        /// </returns>
        /// <remarks>
        /// API này thực hiện các bước sau:
        /// 1. Validate thông tin đầu vào (email và password không được rỗng)
        /// 2. Tìm người dùng theo email hoặc username
        /// 3. Xác minh mật khẩu bằng BCrypt
        /// 4. Trả về thông tin người dùng và token nếu đăng nhập thành công
        /// 
        /// Hỗ trợ đăng nhập bằng cả email và username trong trường Email.
        /// </remarks>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            // Validate đầu vào
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Vui lòng nhập email và mật khẩu" 
                });
            }

            // Tìm người dùng theo email hoặc username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Email);

            if (user == null)
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Email hoặc mật khẩu không đúng" 
                });
            }

            // Xác minh mật khẩu
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Email hoặc mật khẩu không đúng" 
                });
            }

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = "temp_token_" + user.Id, // Tạm thời dùng token đơn giản
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
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
            var user = await _context.Users.FirstOrDefaultAsync();
            
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng");
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            });
        }
    }
}