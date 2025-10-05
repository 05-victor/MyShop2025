using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Shared;
using MyShop.Shared.DTOs;
using MyShop.Data.Entities;
using BCrypt.Net;

namespace MyShop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ShopContext _context;
        
        public AuthController(ShopContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            // Validate input
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

            // Check if user already exists
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

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
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

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Vui lòng nhập email và mật khẩu" 
                });
            }

            // Find user by email or username
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

            // Verify password
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