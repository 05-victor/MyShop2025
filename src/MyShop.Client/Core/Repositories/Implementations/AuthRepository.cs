using MyShop.Client.ApiServer;
using MyShop.Client.Core.Adapters;
using MyShop.Client.Core.Common;
using MyShop.Client.Core.Repositories.Interfaces;
using MyShop.Client.Models;
using MyShop.Shared.DTOs.Requests;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyShop.Client.Core.Repositories.Implementations
{
    /// <summary>
    /// Implementation của IAuthRepository
    /// Wraps IAuthApi và transform DTOs → Domain Models
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly IAuthApi _authApi;

        public AuthRepository(IAuthApi authApi)
        {
            _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
        }

        public async Task<Result<User>> LoginAsync(string usernameOrEmail, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    UsernameOrEmail = usernameOrEmail,
                    Password = password
                };

                var response = await _authApi.LoginAsync(request);

                if (response?.Success == true && response.Result != null)
                {
                    // Transform DTO → Domain Model using Adapter
                    var user = UserAdapter.FromLoginResponse(response.Result);
                    return Result<User>.Success(user);
                }

                return Result<User>.Failure(response?.Message ?? "Login failed");
            }
            catch (Refit.ApiException apiEx)
            {
                var errorMessage = MapApiError(apiEx);
                return Result<User>.Failure(errorMessage, apiEx);
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
                return Result<User>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.Message}");
                return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
            }
        }

        public async Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
        {
            try
            {
                var request = new CreateUserRequest
                {
                    Username = username,
                    Email = email,
                    Password = password
                };

                var response = await _authApi.RegisterAsync(request);

                if (response?.Success == true && response.Result != null)
                {
                    // Note: Register response doesn't include token, need to login after
                    // Create temporary user model
                    var user = new User
                    {
                        Id = response.Result.Id,
                        Username = username,
                        Email = email,
                        CreatedAt = DateTime.Now
                    };
                    return Result<User>.Success(user);
                }

                return Result<User>.Failure(response?.Message ?? "Registration failed");
            }
            catch (Refit.ApiException apiEx)
            {
                var errorMessage = MapApiError(apiEx);
                return Result<User>.Failure(errorMessage, apiEx);
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
                return Result<User>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.Message}");
                return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
            }
        }

        public async Task<Result<User>> GetCurrentUserAsync()
        {
            try
            {
                var response = await _authApi.GetMeAsync();

                if (response?.Success == true && response.Result != null)
                {
                    // Get token from credential helper (will inject later if needed)
                    var token = Helpers.CredentialHelper.GetToken() ?? string.Empty;
                    var user = UserAdapter.FromUserInfoResponse(response.Result, token);
                    return Result<User>.Success(user);
                }

                return Result<User>.Failure(response?.Message ?? "Failed to get user info");
            }
            catch (Refit.ApiException apiEx)
            {
                var errorMessage = MapApiError(apiEx);
                return Result<User>.Failure(errorMessage, apiEx);
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
                return Result<User>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.Message}");
                return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
            }
        }

        /// <summary>
        /// Map API errors thành user-friendly messages
        /// </summary>
        private string MapApiError(Refit.ApiException apiEx)
        {
            System.Diagnostics.Debug.WriteLine($"API Error: {apiEx.StatusCode} - {apiEx.Content}");

            return apiEx.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Invalid username or password",
                System.Net.HttpStatusCode.NotFound => "Account not found",
                System.Net.HttpStatusCode.BadRequest => ParseBadRequestError(apiEx.Content),
                _ => "Network error. Please check your connection"
            };
        }

        private string ParseBadRequestError(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return "Invalid request";

            if (content.Contains("username", StringComparison.OrdinalIgnoreCase))
                return "Username already exists";
            
            if (content.Contains("email", StringComparison.OrdinalIgnoreCase))
                return "Email already registered";

            return "Invalid request data";
        }
    }
}

