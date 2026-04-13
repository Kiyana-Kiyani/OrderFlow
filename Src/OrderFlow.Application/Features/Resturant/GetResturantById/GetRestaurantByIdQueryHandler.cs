using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Resturant.GetResturantById
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
            var result = await _dbContext.Restaurants
                .Where(x => x.Id == query.Id)
                .Select(x => new GetRestaurantByIdResponse(
                    x.Id,
                    x.Name,
                    x.Address,
                    x.Description,
                    x.IsActive))
                .FirstOrDefaultAsync(cancellationToken);

            if (result is null)
            {
                throw new KeyNotFoundException($"Restaurant with Id {query.Id} not found.");
            }
            return result;

        }
    }
}
