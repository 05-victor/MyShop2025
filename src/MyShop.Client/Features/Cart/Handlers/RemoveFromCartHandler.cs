using MediatR;
using MyShop.Client.Features.Cart.Commands;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Features.Cart.Handlers;

/// <summary>
/// Handler for RemoveFromCartCommand
/// </summary>
public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand, Result<bool>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IAuthRepository _authRepository;

    public RemoveFromCartHandler(ICartRepository cartRepository, IAuthRepository authRepository)
    {
        _cartRepository = cartRepository;
        _authRepository = authRepository;
    }

    public async Task<Result<bool>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (userResult == null || !userResult.IsSuccess || userResult.Data == null)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            var result = await _cartRepository.RemoveFromCartAsync(userResult.Data.Id, request.ProductId);
            
        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(result.ErrorMessage);
        }            return Result<bool>.Success(result.Data);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error removing from cart: {ex.Message}");
        }
    }
}
