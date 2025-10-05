using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using MyShop.Shared.DTOs;

namespace MyShop.Client.Services {
    public class AuthService : IAuthService {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthService(HttpClient httpClient) {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<AuthResponse> LoginAsync(string email, string password) {
            // --- MOCK SUCCESS FOR DEVELOPMENT/TESTING ---
            // This allows navigation testing without server connection
            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password)) {
                await Task.Delay(500); // Simulate network delay
                return new AuthResponse {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = "MOCK_TOKEN_123",
                    User = new UserDto {
                        Id = 1,
                        Username = email,
                        Email = email,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            // --- END MOCK ---

            /* UNCOMMENT THIS SECTION WHEN YOU WANT TO TEST REAL SERVER CONNECTION
            var request = new LoginRequest
            {
                Email = email,
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

            return new AuthResponse { Success = false, Message = "Vui lòng nhập email và mật khẩu" };
        }

        public async Task<AuthResponse> RegisterAsync(string username, string email, string password) {
            // --- MOCK SUCCESS FOR DEVELOPMENT/TESTING ---
            // This allows navigation testing without server connection
            if (!string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(password)) {
                await Task.Delay(500); // Simulate network delay
                return new AuthResponse {
                    Success = true,
                    Message = "Đăng ký thành công",
                    Token = "MOCK_TOKEN_123",
                    User = new UserDto {
                        Id = 1,
                        Username = username,
                        Email = email,
                        CreatedAt = DateTime.UtcNow
                    }
                };
            }
            // --- END MOCK ---

            /* UNCOMMENT THIS SECTION WHEN YOU WANT TO TEST REAL SERVER CONNECTION
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

            return new AuthResponse { Success = false, Message = "Vui lòng điền đầy đủ thông tin" };
        }

        public async Task<UserDto?> GetCurrentUserAsync() {
            try {
                var response = await _httpClient.GetAsync("api/auth/me");
                if (response.IsSuccessStatusCode) {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
                }
            }
            catch (Exception) {
                // Log error if needed
            }

            return null;
        }
    }
}