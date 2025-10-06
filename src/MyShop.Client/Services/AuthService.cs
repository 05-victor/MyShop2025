using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using MyShop.Shared.DTOs;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Service chịu trách nhiệm xử lý các hoạt động xác thực người dùng.
    /// Cung cấp các phương thức để đăng nhập, đăng ký và lấy thông tin người dùng hiện tại.
    /// </summary>
    /// <remarks>
    /// Service này giao tiếp với API xác thực backend và xử lý:
    /// - Đăng nhập người dùng với username/email và mật khẩu
    /// - Đăng ký người dùng mới
    /// - Lấy thông tin người dùng được xác thực hiện tại
    /// - JSON serialization/deserialization cho giao tiếp API
    /// - Xử lý lỗi cho mạng và thất bại xác thực
    /// 
    /// Hiện tại bao gồm các implementation giả lập cho mục đích phát triển/kiểm thử.
    /// </remarks>
    public class AuthService : IAuthService
    {
        #region Private Fields

        /// <summary>
        /// HTTP client để thực hiện các API request tới các endpoint xác thực
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Tùy chọn JSON serialization để giao tiếp API nhất quán
        /// </summary>
        private readonly JsonSerializerOptions _jsonOptions;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="AuthService"/>.
        /// </summary>
        /// <param name="httpClient">HTTP client để thực hiện API requests</param>
        /// <exception cref="ArgumentNullException">Được throw khi httpClient là null</exception>
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Xác thực người dùng với username/email và mật khẩu của họ một cách bất đồng bộ.
        /// </summary>
        /// <param name="usernameOrEmail">Username hoặc địa chỉ email của người dùng</param>
        /// <param name="password">Mật khẩu của người dùng</param>
        /// <returns>
        /// Một task đại diện cho hoạt động bất đồng bộ. Kết quả task chứa
        /// một <see cref="AuthResponse"/> cho biết thành công hoặc thất bại với thông báo phù hợp.
        /// </returns>
        /// <remarks>
        /// Phương thức này hiện tại sử dụng implementation giả lập cho phát triển/kiểm thử.
        /// Khi sẵn sàng cho production, hãy bỏ comment phần implementation API thực.
        /// 
        /// Implementation giả lập:
        /// - Mô phỏng độ trễ mạng 500ms
        /// - Trả về thành công cho bất kỳ username/email và mật khẩu không rỗng nào
        /// - Tạo token và dữ liệu người dùng giả lập
        /// 
        /// Implementation production sẽ:
        /// - Gửi POST request tới endpoint /api/auth/login
        /// - Xử lý response thành công và lỗi một cách phù hợp
        /// - Bao gồm xử lý lỗi thích hợp cho thất bại mạng
        /// </remarks>
        /// <example>
        /// <code>
        /// var authService = new AuthService(httpClient);
        /// var result = await authService.LoginAsync("user@example.com", "password123");
        /// 
        /// if (result.Success)
        /// {
        ///     Console.WriteLine($"Chào mừng {result.User.Username}!");
        ///     // Lưu result.Token cho các API call tương lai
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"Đăng nhập thất bại: {result.Message}");
        /// }
        /// </code>
        /// </example>
        public async Task<AuthResponse> LoginAsync(string usernameOrEmail, string password)
        {
            // Validation đầu vào
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = "Vui lòng nhập email/tên đăng nhập và mật khẩu" 
                };
            }

            // --- GIẢ LẬP THÀNH CÔNG CHO PHÁT TRIỂN/KIỂM THỬ ---
            // Điều này cho phép kiểm thử điều hướng mà không cần kết nối server
            await Task.Delay(500); // Mô phỏng độ trễ mạng
            return new AuthResponse
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = "MOCK_TOKEN_123",
                User = new UserDto
                {
                    Id = 1,
                    Username = usernameOrEmail,
                    Email = usernameOrEmail.Contains("@") ? usernameOrEmail : $"{usernameOrEmail}@example.com",
                    CreatedAt = DateTime.UtcNow
                }
            };
            // --- KẾT THÚC GIẢ LẬP ---

            /* BỎ COMMENT PHẦN NÀY KHI BẠN MUỐN KIỂM THỬ KẾT NỐI SERVER THỰC
            var request = new LoginRequest
            {
                Email = usernameOrEmail, // Server sẽ xử lý cả email và username
                Password = password
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<AuthResponse>(responseContent, _jsonOptions) 
                           ?? new AuthResponse { Success = false, Message = "Invalid response" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, _jsonOptions);
                    return errorResponse ?? new AuthResponse { Success = false, Message = "Đăng nhập thất bại" };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
            */
        }

        /// <summary>
        /// Đăng ký người dùng mới với thông tin được cung cấp một cách bất đồng bộ.
        /// </summary>
        /// <param name="username">Username mong muốn cho người dùng mới</param>
        /// <param name="email">Địa chỉ email cho người dùng mới</param>
        /// <param name="password">Mật khẩu cho người dùng mới</param>
        /// <returns>
        /// Một task đại diện cho hoạt động bất đồng bộ. Kết quả task chứa
        /// một <see cref="AuthResponse"/> cho biết thành công hoặc thất bại với thông báo phù hợp.
        /// </returns>
        /// <remarks>
        /// Phương thức này hiện tại sử dụng implementation giả lập cho phát triển/kiểm thử.
        /// Khi sẵn sàng cho production, hãy bỏ comment phần implementation API thực.
        /// 
        /// Implementation giả lập:
        /// - Mô phỏng độ trễ mạng 500ms
        /// - Trả về thành công cho bất kỳ username, email và mật khẩu không rỗng nào
        /// - Tạo token và dữ liệu người dùng giả lập
        /// 
        /// Implementation production sẽ:
        /// - Gửi POST request tới endpoint /api/auth/register
        /// - Xử lý response thành công và lỗi một cách phù hợp
        /// - Bao gồm xử lý lỗi thích hợp cho thất bại mạng
        /// - Xử lý lỗi validation (email trùng lặp, mật khẩu yếu, v.v.)
        /// </remarks>
        /// <example>
        /// <code>
        /// var authService = new AuthService(httpClient);
        /// var result = await authService.RegisterAsync("johndoe", "john@example.com", "password123");
        /// 
        /// if (result.Success)
        /// {
        ///     Console.WriteLine($"Đăng ký thành công! Chào mừng {result.User.Username}!");
        ///     // Lưu result.Token cho các API call tương lai
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"Đăng ký thất bại: {result.Message}");
        /// }
        /// </code>
        /// </example>
        public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
        {
            // Validation đầu vào
            if (string.IsNullOrWhiteSpace(username) || 
                string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password))
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = "Vui lòng điền đầy đủ thông tin" 
                };
            }

            // --- GIẢ LẬP THÀNH CÔNG CHO PHÁT TRIỂN/KIỂM THỬ ---
            // Điều này cho phép kiểm thử điều hướng mà không cần kết nối server
            await Task.Delay(500); // Mô phỏng độ trễ mạng
            return new AuthResponse
            {
                Success = true,
                Message = "Đăng ký thành công",
                Token = "MOCK_TOKEN_123",
                User = new UserDto
                {
                    Id = 1,
                    Username = username,
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                }
            };
            // --- KẾT THÚC GIẢ LẬP ---

            /* BỎ COMMENT PHẦN NÀY KHI BẠN MUỐN KIỂM THỬ KẾT NỐI SERVER THỰC
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<AuthResponse>(responseContent, _jsonOptions) 
                           ?? new AuthResponse { Success = false, Message = "Invalid response" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, _jsonOptions);
                    return errorResponse ?? new AuthResponse { Success = false, Message = "Đăng ký thất bại" };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
            */
        }

        /// <summary>
        /// Lấy thông tin người dùng được xác thực hiện tại một cách bất đồng bộ.
        /// </summary>
        /// <returns>
        /// Một task đại diện cho hoạt động bất đồng bộ. Kết quả task chứa
        /// một <see cref="UserDto"/> với thông tin người dùng nếu được xác thực,
        /// hoặc null nếu không được xác thực hoặc nếu có lỗi xảy ra.
        /// </returns>
        /// <remarks>
        /// Phương thức này yêu cầu một token xác thực hợp lệ phải có mặt trong
        /// default headers của HTTP client (thường được đặt sau đăng nhập/đăng ký thành công).
        /// 
        /// Phương thức sẽ:
        /// - Gửi GET request tới endpoint /api/auth/me
        /// - Trả về thông tin người dùng nếu request thành công
        /// - Trả về null nếu người dùng không được xác thực hoặc nếu có lỗi xảy ra
        /// - Xử lý lỗi mạng một cách nhẹ nhàng bằng cách trả về null
        /// </remarks>
        /// <example>
        /// <code>
        /// var authService = new AuthService(httpClient);
        /// // Giả sử người dùng đã đăng nhập và token được đặt
        /// var currentUser = await authService.GetCurrentUserAsync();
        /// 
        /// if (currentUser != null)
        /// {
        ///     Console.WriteLine($"Người dùng hiện tại: {currentUser.Username} ({currentUser.Email})");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Người dùng không được xác thực hoặc có lỗi xảy ra");
        /// }
        /// </code>
        /// </example>
        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/auth/me");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
                }
            }
            catch (Exception)
            {
                // Log lỗi nếu cần
                // Trong production, cân nhắc log exception này cho mục đích debug
                // logger.LogError(ex, "Thất bại khi lấy thông tin người dùng hiện tại");
            }

            return null;
        }

        #endregion
    }
}