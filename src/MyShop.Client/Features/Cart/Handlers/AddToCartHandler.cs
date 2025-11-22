using MediatR;
using MyShop.Client.Features.Cart.Commands;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Features.Cart.Handlers;

/// <summary>
/// Handler for AddToCartCommand
/// </summary>
public class AddToCartHandler : IRequestHandler<AddToCartCommand, Result<bool>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IAuthRepository _authRepository;

    public AddToCartHandler(ICartRepository cartRepository, IAuthRepository authRepository)
    {
        _cartRepository = cartRepository;
        _authRepository = authRepository;
    }

    public async Task<Result<bool>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user ID from auth
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (userResult == null || !userResult.IsSuccess || userResult.Data == null)
            {
                return Result<bool>.Failure("User not authenticated");
            }

            var success = await _cartRepository.AddToCartAsync(userResult.Data.Id, request.ProductId, request.Quantity);
            
            return success 
                ? Result<bool>.Success(true) 
                : Result<bool>.Failure("Failed to add item to cart");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error adding to cart: {ex.Message}");
        }
    }
}
