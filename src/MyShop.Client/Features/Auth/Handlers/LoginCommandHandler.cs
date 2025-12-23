using MediatR;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Client.Services;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Auth.Handlers;

/// <summary>
/// Handler that processes LoginCommand.
/// Authenticates user and saves token if RememberMe is enabled.
/// Calls GetMe endpoint to load complete user profile and cache it.
/// </summary>
public class LoginCommandHandler : IRequestHandler<Commands.LoginCommand, Result<User>>
{
    private readonly IAuthRepository _authRepository;
    private readonly ICredentialStorage _credentialStorage;
    private readonly ICurrentUserService _currentUserService;

    public LoginCommandHandler(
        IAuthRepository authRepository,
        ICredentialStorage credentialStorage,
        ICurrentUserService currentUserService)
    {
        _authRepository = authRepository;
        _credentialStorage = credentialStorage;
        _currentUserService = currentUserService;
    }

    public async Task<Result<User>> Handle(Commands.LoginCommand request, CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] Started login for user: {request.Username}");

        // Call repository (already returns Result<User> from DTO mapping)
        var result = await _authRepository.LoginAsync(
            request.Username.Trim(),
            request.Password);

        System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] Login result: IsSuccess={result.IsSuccess}, Username={result.Data?.Username}");

        // Save token if remember me is checked
        if (result.IsSuccess && result.Data != null && request.RememberMe)
        {
            await _credentialStorage.SaveToken(result.Data.Token);
            System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] Token saved (RememberMe=true)");
        }

        // After successful login, fetch full user profile from GetMe endpoint
        if (result.IsSuccess && result.Data != null)
        {
            System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] Calling GetMeAsync to fetch complete user profile...");
            var userResult = await _authRepository.GetCurrentUserAsync();

            System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] GetMe result: IsSuccess={userResult.IsSuccess}, Error={userResult.ErrorMessage}");

            if (userResult.IsSuccess && userResult.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] ✓ GetMe successful - User data:");
                System.Diagnostics.Debug.WriteLine($"  - Id: {userResult.Data.Id}");
                System.Diagnostics.Debug.WriteLine($"  - Username: {userResult.Data.Username}");
                System.Diagnostics.Debug.WriteLine($"  - Email: {userResult.Data.Email}");
                System.Diagnostics.Debug.WriteLine($"  - FullName: {userResult.Data.FullName}");
                System.Diagnostics.Debug.WriteLine($"  - PhoneNumber: {userResult.Data.PhoneNumber}");
                System.Diagnostics.Debug.WriteLine($"  - Address: {userResult.Data.Address}");
                System.Diagnostics.Debug.WriteLine($"  - Avatar: {userResult.Data.Avatar}");
                System.Diagnostics.Debug.WriteLine($"  - IsEmailVerified: {userResult.Data.IsEmailVerified}");
                System.Diagnostics.Debug.WriteLine($"  - IsTrialActive: {userResult.Data.IsTrialActive}");
                System.Diagnostics.Debug.WriteLine($"  - Roles: {string.Join(", ", userResult.Data.Roles)}");

                // Cache the complete user profile
                _currentUserService.SetCurrentUser(userResult.Data);
                System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] User cached successfully");
                return Result<User>.Success(userResult.Data);
            }
            else
            {
                // GetMe failed, but login succeeded - cache the partial user from login response
                System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] ✗ GetMe failed: {userResult.ErrorMessage}");
                _currentUserService.SetCurrentUser(result.Data);
                System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] Using partial user from login response instead");
                return result;
            }
        }

        System.Diagnostics.Debug.WriteLine($"[LoginCommandHandler] Login failed");
        return result;
    }
}
