using MediatR;
using MyShop.Client.Features.Profile.Commands;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Profile.Handlers;

/// <summary>
/// Handler for UpdateProfileCommand
/// </summary>
public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result<User>>
{
    private readonly IUserRepository _userRepository;

    public UpdateProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<User>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updateRequest = new UpdateProfileRequest
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address
            };

            var result = await _userRepository.UpdateProfileAsync(updateRequest);
            return result;
        }
        catch (Exception ex)
        {
            return Result<User>.Failure($"Error updating profile: {ex.Message}");
        }
    }
}
