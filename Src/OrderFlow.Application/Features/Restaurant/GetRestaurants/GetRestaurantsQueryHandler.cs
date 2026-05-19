using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Restaurant.GetRestaurants
{
    public class GetRestaurantsQueryHandler : IRequestHandler<GetRestaurantsQuery, IReadOnlyList<GetRestaurantsResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetRestaurantsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<GetRestaurantsResponse>> Handle(GetRestaurantsQuery request, CancellationToken cancellationToken)
        {
            var restaurants = await _context.Restaurants
              .Select(r => new GetRestaurantsResponse
              (
                  r.Id,
                  r.Name,
                  r.Address,
                  r.Description,
                  r.IsActive
              )).ToListAsync(cancellationToken);

            return restaurants;
        }
    }
}
