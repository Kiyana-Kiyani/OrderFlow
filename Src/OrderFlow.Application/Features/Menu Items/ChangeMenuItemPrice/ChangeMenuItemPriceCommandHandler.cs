using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Menu_Items.ChangeMenuItemPrice
{
    public class ChangeMenuItemPriceCommandHandler : IRequestHandler<ChangeMenuItemPriceCommand>
    {

        private readonly IApplicationDbContext _dbContext;

        public ChangeMenuItemPriceCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ChangeMenuItemPriceCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants.Include(r => r.MenuItems).FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);
            if (restaurant == null)
            {
                throw new Exception("Restaurant not found");
            }

            restaurant.ChangeMenuItemPrice(request.MenuItemId, request.Price);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
