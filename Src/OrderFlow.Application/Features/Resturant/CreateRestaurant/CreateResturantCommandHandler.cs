using MediatR;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Features.Resturant.CreateRestaurant
{
    public class CreateResturantCommandHandler : IRequestHandler<CreateRestaurantCommand , CreateRestaurantResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IApplicationDbContext _dbContext;


        public CreateResturantCommandHandler(ICurrentUser currentUser, IApplicationDbContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
        }

        public async Task<CreateRestaurantResponse> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = new Restaurant
            (
                name: request.Name,
                address: request.Address,
                description: request.Description,
                ownerUserId: _currentUser.UserId
            );

            _dbContext.Restaurants.Add(restaurant);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateRestaurantResponse(restaurant.Id);

        }
    }
}
