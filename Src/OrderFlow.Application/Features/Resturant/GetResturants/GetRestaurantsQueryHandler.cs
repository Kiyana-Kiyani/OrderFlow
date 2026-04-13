using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Resturant.GetResturants
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
          var result = await _context.Restaurants.AsNoTracking()
            .Select(r => new GetRestaurantsResponse
            (
                r.Id,
                r.Name,
                r.Address,
                r.Description ,
                r.IsActive
            )).ToListAsync(cancellationToken);

            return result;
        }
    }
}
