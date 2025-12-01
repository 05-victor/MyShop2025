using MediatR;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Auth.Handlers;

/// <summary>
/// Handler that processes RegisterCommand.
/// Creates a new user account with the provided information.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<Commands.RegisterCommand, Result<User>>
{
    private readonly IAuthRepository _authRepository;

    public RegisterCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<Result<User>> Handle(Commands.RegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await _authRepository.RegisterAsync(
            request.Username,
            request.Email,
            request.PhoneNumber,
            request.Password,
            request.Role);

        return result;
    }
}
