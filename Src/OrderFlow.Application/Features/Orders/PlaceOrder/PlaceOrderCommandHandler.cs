using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Features.Orders.PlaceOrder
{
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public PlaceOrderCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<PlaceOrderResponse> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {


            var restaurant = _dbContext.Restaurants.AsNoTracking()
                .FirstOrDefault(r => r.Id == request.RestaurantId && r.IsActive);

            if (restaurant is null)
                throw new ArgumentException("Restaurant doesnt exict");


            if (!restaurant.IsActive)
                throw new ArgumentException("Restaurant isnt active");

            var order = new CustomerOrder(_currentUser.UserId, request.RestaurantId, restaurant.Name);

            var menuItemIds = request.Items.Select(c => c.MenuItemId).Distinct().ToList();

            var menuItems = await _dbContext.MenuItems
                .Where(x => x.RestaurantId == request.RestaurantId && menuItemIds.Contains(x.Id))
                .AsNoTracking().ToListAsync(cancellationToken);


            if (menuItems.Count != menuItemIds.Count)
                throw new ArgumentException("One or more menu items do not exist.");

            if (menuItems.Any(m => !m.IsAvailable))
                throw new ArgumentException("One or more menu items are not available.");

            foreach (var item in request.Items)
            {
                var price = menuItems.Where(x => x.Id == item.MenuItemId).Select(x => x.Price).FirstOrDefault();
                var name = menuItems.Where(x => x.Id == item.MenuItemId).Select(x => x.Name).FirstOrDefault();
                order.AddOrderItem(item.Quantity, price, item.MenuItemId, name );
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new PlaceOrderResponse(order.Id, order.Status, order.TotalAmount, order.CreatedAt);
        }
    }
}
