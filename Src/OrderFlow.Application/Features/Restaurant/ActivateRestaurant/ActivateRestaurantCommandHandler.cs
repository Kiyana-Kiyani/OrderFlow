using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Features.Orders.PlaceOrder;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.Restaurant.ActivateRestaurant
{
    public class ActivateRestaurantCommandHandler : IRequestHandler<ActivateRestaurantCommand>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<PlaceOrderCommandHandler> _logger;


        public ActivateRestaurantCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser,
            IAuthorizationService authorizationService, ILogger<PlaceOrderCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task Handle(ActivateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants.FindAsync(new object[] { request.RestaurantId }, cancellationToken);
            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.RestaurantId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());

            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            restaurant.Activate();
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Restaurant {RestaurantId} has been activated and is now visible to customers. Action by User {UserId}.",
                restaurant.Id, _currentUser.UserId);
        }
    }
}
