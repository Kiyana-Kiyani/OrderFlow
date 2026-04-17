using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.MenuItems.AddMenuItem
{
    public class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, AddMenuItemResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;

        public AddMenuItemCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IAuthorizationService authorizationService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
        }
        public async Task<AddMenuItemResponse> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
                 .Include(x => x.MenuItems)
                 .FirstOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);

            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.RestaurantId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            var menuItemId = restaurant.AddMenuItem(
                request.Name,
                request.Price,
                request.Description);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new AddMenuItemResponse(menuItemId);
        }
    }
}
