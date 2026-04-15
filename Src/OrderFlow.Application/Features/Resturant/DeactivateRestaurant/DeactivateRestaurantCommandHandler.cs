using MediatR;
using Microsoft.AspNetCore.Authorization;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.Resturant.DeactivateRestaurant
{
    public class DeactivateRestaurantCommandHandler : IRequestHandler<DeactivateRestaurantCommand>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;
        public DeactivateRestaurantCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IAuthorizationService authorizationService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
        }

        public async Task Handle(DeactivateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants.FindAsync(new object[] { request.RestaurantId }, cancellationToken);
            if (restaurant == null)
                throw new KeyNotFoundException("Restaurant not found.");

            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            restaurant.Deactivate();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}