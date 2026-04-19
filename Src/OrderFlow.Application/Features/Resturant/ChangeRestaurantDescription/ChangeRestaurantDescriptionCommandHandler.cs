using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Features.Resturant.ChangeRestaurantAddress;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantDescription
{
    public class ChangeRestaurantDescriptionCommandHandler : IRequestHandler<ChangeRestaurantDescriptionCommand>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<ChangeRestaurantAddressCommandHandler> _logger;


        public ChangeRestaurantDescriptionCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser,
            IAuthorizationService authorizationService, ILogger<ChangeRestaurantAddressCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task Handle(ChangeRestaurantDescriptionCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants.FindAsync(new object[] { request.RestaurantId }, cancellationToken);
            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.RestaurantId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            restaurant.ChangeDescription(request.NewDescription);

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Description of restaurant {RestaurantId} updated by user {UserId}.",
              restaurant.Id, _currentUser.UserId);
        }
    }
}