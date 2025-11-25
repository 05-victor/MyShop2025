using MediatR;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Auth.Handlers;

/// <summary>
/// Handler xử lý LoginCommand
/// </summary>
public class LoginCommandHandler : IRequestHandler<Commands.LoginCommand, Result<User>>
{
    private readonly IAuthRepository _authRepository;
    private readonly ICredentialStorage _credentialStorage;

    public LoginCommandHandler(
        IAuthRepository authRepository,
        ICredentialStorage credentialStorage)
    {
        _authRepository = authRepository;
        _credentialStorage = credentialStorage;
    }

    public async Task<Result<User>> Handle(Commands.LoginCommand request, CancellationToken cancellationToken)
    {
        // Call repository (already returns Result<User> from DTO mapping)
        var result = await _authRepository.LoginAsync(
            request.Username.Trim(), 
            request.Password);

        // Save token if remember me is checked
        if (result.IsSuccess && result.Data != null && request.RememberMe)
        {
            await _credentialStorage.SaveToken(result.Data.Token);
        }

        return result;
    }
}
