using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Common.Exceptions;

namespace OrderFlow.Application.Features.Restaurant.GetRestaurantById
{
    public class GetRestaurantByIdQueryHandler : IRequestHandler<GetRestaurantByIdQuery, GetRestaurantByIdResponse>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetRestaurantByIdQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetRestaurantByIdResponse> Handle(GetRestaurantByIdQuery query, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
                .Where(x => x.Id == query.Id)
                .Select(x => new GetRestaurantByIdResponse(
                    x.Id,
                    x.Name,
                    x.Address,
                    x.Description,
                    x.IsActive))
                .FirstOrDefaultAsync(cancellationToken);

            if (restaurant is null)
                throw new NotFoundException("Restaurant", query.Id);

            return restaurant;

        }
    }
}
