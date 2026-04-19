using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.MenuItems.ChangeMenuItemDescription
{
    public class ChangeMenuItemDescriptionCommandHandler : IRequestHandler<ChangeMenuItemDescriptionCommand>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<ChangeMenuItemDescriptionCommandHandler> _logger;

        public ChangeMenuItemDescriptionCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser,
            IAuthorizationService authorizationService, ILogger<ChangeMenuItemDescriptionCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task Handle(ChangeMenuItemDescriptionCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.RestaurantId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            restaurant.ChangeMenuItemDescription(request.MenuItemId, request.Description);

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Description of MenuItem {MenuItemId} in restaurant {RestaurantId} updated by user {UserId}.",
                 request.MenuItemId, restaurant.Id, _currentUser.UserId);
        }
    }
}
