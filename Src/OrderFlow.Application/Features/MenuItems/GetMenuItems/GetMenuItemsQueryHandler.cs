using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.MenuItems.GetMenuItems
{
    public class GetMenuItemsQueryHandler : IRequestHandler<GetMenuItemsQuery, IReadOnlyList<GetMenuItemsResponse>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMenuItemsQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<GetMenuItemsResponse>> Handle(GetMenuItemsQuery request, CancellationToken cancellationToken)
        {
            var items = await _dbContext.MenuItems
                   .Where(x => x.RestaurantId == request.RestaurantId)
                   .Select(x => new GetMenuItemsResponse(
                       x.Id,
                       x.Name,
                       x.Description,
                       x.Price,
                       x.IsAvailable
                   )).ToListAsync(cancellationToken);

            return items;
        }
    }
}
