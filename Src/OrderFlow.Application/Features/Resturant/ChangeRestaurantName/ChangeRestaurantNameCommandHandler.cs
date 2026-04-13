using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantName
{
    public class ChangeRestaurantNameCommandHandler : IRequestHandler<ChangeRestaurantNameCommand>
    {
        private readonly IApplicationDbContext _dbContext;

        public ChangeRestaurantNameCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ChangeRestaurantNameCommand request, CancellationToken cancellationToken)
        {
            await _dbContext.Restaurants.Where(x => x.Id == request.RestaurantId)
                .ExecuteUpdateAsync(x => x.SetProperty(r => r.Name, request.NewName), cancellationToken);
        }
    }
}