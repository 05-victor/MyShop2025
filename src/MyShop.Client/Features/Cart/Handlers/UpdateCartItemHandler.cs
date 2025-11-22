using MediatR;
using MyShop.Client.Features.Cart.Commands;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Features.Cart.Handlers;

/// <summary>
/// Handler for UpdateCartItemCommand
/// </summary>
public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, Result<bool>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IAuthRepository _authRepository;

    public UpdateCartItemHandler(ICartRepository cartRepository, IAuthRepository authRepository)
    {
        _cartRepository = cartRepository;
        _authRepository = authRepository;
    }

    public async Task<Result<bool>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (userResult == null || !userResult.IsSuccess || userResult.Data == null)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            if (request.NewQuantity <= 0)
            {
                return Result<bool>.Failure("Quantity must be greater than 0");
            }

            var success = await _cartRepository.UpdateQuantityAsync(userResult.Data.Id, request.ProductId, request.NewQuantity);
            
            return success 
                ? Result<bool>.Success(true) 
                : Result<bool>.Failure("Failed to update cart item");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating cart item: {ex.Message}");
        }
    }
}
