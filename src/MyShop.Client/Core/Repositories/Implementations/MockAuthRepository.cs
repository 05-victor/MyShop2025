using MyShop.Client.Core.Common;
using MyShop.Client.Core.MockData;
using MyShop.Client.Core.Repositories.Interfaces;
using MyShop.Client.Models;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Core.Repositories.Implementations
{
    /// <summary>
    /// Mock implementation of IAuthRepository for demo purposes
    /// </summary>
    public class MockAuthRepository : IAuthRepository
    {
        public async Task<Result<User>> LoginAsync(string usernameOrEmail, string password)
        {
            try
            {
                return await MockAuthData.LoginAsync(usernameOrEmail, password);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Mock Login Error: {ex.Message}");
                return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
            }
        }

        public async Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
        {
            try
            {
                return await MockAuthData.RegisterAsync(username, email, phoneNumber, password, role);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Mock Register Error: {ex.Message}");
                return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
            }
        }

        public async Task<Result<User>> GetCurrentUserAsync()
        {
            try
            {
                var token = Helpers.CredentialHelper.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Result<User>.Failure("No authentication token found");
                }

                return await MockAuthData.GetCurrentUserAsync(token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Mock GetCurrentUser Error: {ex.Message}");
                return Result<User>.Failure("An unexpected error occurred. Please try again.", ex);
            }
        }
    }
}
