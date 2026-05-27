using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Restaurant.CreateRestaurant
{
    public class CreateRestaurantCommandHandler : IRequestHandler<CreateRestaurantCommand, CreateRestaurantResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<CreateRestaurantCommandHandler> _logger;

        public CreateRestaurantCommandHandler(IApplicationDbContext dbContext, ILogger<CreateRestaurantCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CreateRestaurantResponse> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = new Domain.Entities.Restaurant
            (
                name: request.Name,
                address: request.Address,
                description: request.Description,
                ownerUserId: request.OwnerId,
                latitude: request.Latitude,
                longitude: request.Longitude
            );

            _dbContext.Restaurants.Add(restaurant);

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Restaurant {RestaurantId} ('{RestaurantName}') created successfully. Owner: {OwnerId}.",
                 restaurant.Id, restaurant.Name, restaurant.OwnerUserId);
            return new CreateRestaurantResponse(restaurant.Id);
        }
    }
}
