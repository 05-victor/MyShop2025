using MediatR;
using MyShop.Client.Features.Cart.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Features.Cart.Handlers;

/// <summary>
/// Handler for GetCartQuery
/// </summary>
public class GetCartHandler : IRequestHandler<GetCartQuery, Result<object>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IAuthRepository _authRepository;

    public GetCartHandler(ICartRepository cartRepository, IAuthRepository authRepository)
    {
        _cartRepository = cartRepository;
        _authRepository = authRepository;
    }

    public async Task<Result<object>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (userResult == null || !userResult.IsSuccess || userResult.Data == null)
            {
                return Result<object>.Failure("User not authenticated");
            }

            var cartItems = await _cartRepository.GetCartItemsAsync(userResult.Data.Id);
            var cartSummary = await _cartRepository.GetCartSummaryAsync(userResult.Data.Id);

            var cartData = new
            {
                Items = cartItems,
                Summary = cartSummary
            };
            
            return Result<object>.Success(cartData);
        }
        catch (Exception ex)
        {
            return Result<object>.Failure($"Error loading cart: {ex.Message}");
        }
    }
}
