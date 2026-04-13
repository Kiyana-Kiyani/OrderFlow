using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantDescription
{
    public class ChangeRestaurantDescriptionCommandHandler : IRequestHandler<ChangeRestaurantDescriptionCommand>
    {
        private readonly IApplicationDbContext _dbContext;

        public ChangeRestaurantDescriptionCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ChangeRestaurantDescriptionCommand request, CancellationToken cancellationToken)
        {
            await _dbContext.Restaurants.Where(x => x.Id == request.RestaurantId)
                .ExecuteUpdateAsync(x => x.SetProperty(r => r.Description, request.NewDescription), cancellationToken);
        }
    }
}