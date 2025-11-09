using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Storage;
using MyShop.Shared.Models;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Mocks.Repositories;

/// <summary>
/// Mock implementation of IAuthRepository for demo purposes
/// </summary>
public class MockAuthRepository : IAuthRepository
{
    private readonly ICredentialStorage _credentialStorage;

    public MockAuthRepository(ICredentialStorage credentialStorage)
    {
        _credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
    }
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
            var token = _credentialStorage.GetToken();
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
