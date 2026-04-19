using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.Resturant.DeactivateRestaurant
{
    public class DeactivateRestaurantCommandHandler : IRequestHandler<DeactivateRestaurantCommand>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<DeactivateRestaurantCommandHandler> _logger;
        public DeactivateRestaurantCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser,
            IAuthorizationService authorizationService, ILogger<DeactivateRestaurantCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task Handle(DeactivateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants.FindAsync(new object[] { request.RestaurantId }, cancellationToken);
            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.RestaurantId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            restaurant.Deactivate();
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Restaurant {RestaurantId} has been deactivated (hidden from customers) by User {UserId}.",
                  restaurant.Id, _currentUser.UserId);
        }
    }
}