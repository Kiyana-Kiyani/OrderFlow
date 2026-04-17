using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Common.Exceptions;

namespace OrderFlow.Application.Features.MenuItems.GetMenuItemById
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

            if (menuItem is null)
                throw new NotFoundException("MenuItem", request.MenuItemId);

            return menuItem;
        }
    }
}

