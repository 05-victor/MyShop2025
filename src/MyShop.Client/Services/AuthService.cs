using Microsoft.WindowsAppSDK.Runtime.Packages;
using MyShop.Shared.DTOs;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Service thực hiện các chức năng xác thực người dùng thông qua HTTP API.
    /// </summary>
    /// <remarks>
    /// Service này kết nối với backend API để thực hiện:
    /// - Đăng nhập và đăng ký người dùng
    /// - Quản lý token xác thực
    /// - Lấy thông tin người dùng hiện tại
    /// - Xử lý đăng xuất
    /// 
    /// Sử dụng HttpClient để gọi API và JsonSerializer để xử lý dữ liệu JSON.
    /// </remarks>
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Khởi tạo AuthService với HttpClient.
        /// </summary>
        /// <param name="httpClient">HTTP client để gọi API</param>
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Đăng nhập người dùng.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <returns>Kết quả đăng nhập</returns>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);

                if (response.IsSuccessStatusCode) {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                    return result ?? new LoginResponse { Success = false, Message = "Invalid response" };
                }
                else {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new LoginResponse { Success = false, Message = $"Login failed: {errorContent}" };
                }
            }
            catch (Exception ex) {
                return new LoginResponse { Success = false, Message = $"Network error: {ex.Message}" };
            }

            //// --- MOCK ĐỂ VÀO DASHBOARD ---
            //if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) ||
            //    string.IsNullOrWhiteSpace(request.Password)) {
            //    return new LoginResponse {
            //        Success = false,
            //        Message = "Vui lòng điền đầy đủ thông tin"
            //    };
            //}

            //// UNCOMMENT THIS SECTION WHEN YOU WANT TO TEST REAL SERVER CONNECTION
            //// --- GIẢ LẬP THÀNH CÔNG CHO PHÁT TRIỂN/KIỂM THỬ ---
            //// Điều này cho phép kiểm thử điều hướng mà không cần kết nối server
            //await Task.Delay(500); // Mô phỏng độ trễ mạng
            //return new LoginResponse {
            //    Success = true,
            //    Message = "Đăng nhập thành công"
            //};

            //// --- END MOCK ---
        }

        /// <summary>
        /// Đăng ký tài khoản mới.
        /// </summary>
        /// <param name="request">Thông tin đăng ký</param>
        /// <returns>Kết quả đăng ký</returns>
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, _jsonOptions);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(_jsonOptions);
                    return result ?? new RegisterResponse { Success = false, Message = "Invalid response" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new RegisterResponse { Success = false, Message = $"Registration failed: {errorContent}" };
                }
            }
            catch (Exception ex)
            {
                return new RegisterResponse { Success = false, Message = $"Network error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại.
        /// </summary>
        /// <returns>Thông tin người dùng hoặc null</returns>
        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/auth/me");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserInfo>(_jsonOptions);
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Đăng xuất người dùng.
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync("api/auth/logout", null);
            }
            catch
            {
                // Optionally handle/log the error
            }
        }
    }
}