using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.MenuItems.ChangeMenuItemPrice
{
    public class ChangeMenuItemPriceCommandHandler : IRequestHandler<ChangeMenuItemPriceCommand>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<ChangeMenuItemPriceCommandHandler> _logger;


        public ChangeMenuItemPriceCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser,
            IAuthorizationService authorizationService, ILogger<ChangeMenuItemPriceCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task Handle(ChangeMenuItemPriceCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants.Include(r => r.MenuItems).FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.RestaurantId);


            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            restaurant.ChangeMenuItemPrice(request.MenuItemId, request.Price);

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Price of MenuItem {MenuItemId} in restaurant {RestaurantId} updated to {NewPrice} by user {UserId}.",
               request.MenuItemId, restaurant.Id, request.Price, _currentUser.UserId);
        }
    }
}
