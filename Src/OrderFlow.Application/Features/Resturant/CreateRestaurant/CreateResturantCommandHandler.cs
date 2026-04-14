using MediatR;
using OrderFlow.Application.Abstractions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Features.Resturant.CreateRestaurant
{
    public class CreateResturantCommandHandler : IRequestHandler<CreateRestaurantCommand, CreateRestaurantResponse>
    {
        private readonly IApplicationDbContext _dbContext;

        public CreateResturantCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateRestaurantResponse> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = new Restaurant
            (
                name: request.Name,
                address: request.Address,
                description: request.Description,
                ownerUserId: request.OwnerId
            );

            _dbContext.Restaurants.Add(restaurant);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateRestaurantResponse(restaurant.Id);

        }
    }
}
