using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Menu_Items.GetMenuItemById
{
    public class GetMenuItemByIdQueryHandler
        : IRequestHandler<GetMenuItemByIdQuery, MenuItemDetailsResponse>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMenuItemByIdQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MenuItemDetailsResponse> Handle(GetMenuItemByIdQuery request, CancellationToken cancellationToken)
        {
            var menuItem = await _dbContext.MenuItems
                  .Where(x => x.RestaurantId == request.RestaurantId && x.Id == request.MenuItemId)
                  .Select(x => new MenuItemDetailsResponse
                  (
                      x.Id,
                      x.RestaurantId,
                      x.Name,
                      x.Description,
                      x.Price,
                      x.IsAvailable
                  )).FirstOrDefaultAsync(cancellationToken);

            if (menuItem == null)
                throw new InvalidOperationException("Menu item not found.");

            return menuItem;
        }
    }
}

