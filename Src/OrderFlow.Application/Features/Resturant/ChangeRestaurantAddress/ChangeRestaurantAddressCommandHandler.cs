using MediatR;
using Microsoft.AspNetCore.Authorization;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantAddress
{
    public class ChangeRestaurantAddressCommandHandler : IRequestHandler<ChangeRestaurantAddressCommand>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;

        public ChangeRestaurantAddressCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IAuthorizationService authorizationService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
        }

        public async Task Handle(ChangeRestaurantAddressCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants.FindAsync(new object[] { request.RestaurantId }, cancellationToken);
            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.RestaurantId);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());

            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            restaurant.ChangeAddress(request.NewAddress);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
