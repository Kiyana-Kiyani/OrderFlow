using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantAddress
{
    public class ChangeRestaurantAddressCommandHandler : IRequestHandler<ChangeRestaurantAddressCommand>
    {
        private readonly IApplicationDbContext _dbContext;

        public ChangeRestaurantAddressCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ChangeRestaurantAddressCommand request, CancellationToken cancellationToken)
        {
            await _dbContext.Restaurants.Where(x => x.Id == request.RestaurantId)
             .ExecuteUpdateAsync(x => x.SetProperty(r => r.Address, request.NewAddress), cancellationToken);
        }
    }
}
