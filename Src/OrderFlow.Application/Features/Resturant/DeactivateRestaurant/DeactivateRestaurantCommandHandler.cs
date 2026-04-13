using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Resturant.DeactivateRestaurant
{
    public class DeactivateRestaurantCommandHandler : IRequestHandler<DeactivateRestaurantCommand>
    {
        private readonly IApplicationDbContext _dbContext;

        public DeactivateRestaurantCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(DeactivateRestaurantCommand request, CancellationToken cancellationToken)
        {
            await _dbContext.Restaurants.Where(x => x.Id == request.RestaurantId)
                .ExecuteUpdateAsync(x => x.SetProperty(r => r.IsActive, false), cancellationToken);
        }
    }
}