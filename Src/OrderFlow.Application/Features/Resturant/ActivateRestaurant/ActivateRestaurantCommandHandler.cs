using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Resturant.ActivateRestaurant
{
    public class ActivateRestaurantCommandHandler : IRequestHandler<ActivateRestaurantCommand>
    {
        private readonly IApplicationDbContext _dbContext;

        public ActivateRestaurantCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ActivateRestaurantCommand request, CancellationToken cancellationToken)
        {
            await _dbContext.Restaurants.Where(x => x.Id == request.RestaurantId)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsActive, true));
        }
    }
}
