using MediatR;
using Microsoft.AspNetCore.Authorization;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Security.Authorization;

namespace OrderFlow.Application.Features.Resturant.RemoveResturant
{
    public class RemoveRestaurantByIdCommandHandler : IRequestHandler<RemoveRestaurantByIdCommand>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly IAuthorizationService _authorizationService;
        public RemoveRestaurantByIdCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, IAuthorizationService authorizationService)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _authorizationService = authorizationService;
        }

        public async Task Handle(RemoveRestaurantByIdCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants.FindAsync(new object[] { request.Id }, cancellationToken);
            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.Id);

            var authorizationResult = await _authorizationService.AuthorizeAsync(_currentUser.User, restaurant, new ResourceOwnerRequirement());
            if (!authorizationResult.Succeeded)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            _dbContext.Restaurants.Remove(restaurant);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
