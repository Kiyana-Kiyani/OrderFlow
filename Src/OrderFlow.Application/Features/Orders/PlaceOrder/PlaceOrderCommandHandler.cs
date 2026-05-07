using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Features.Orders.PlaceOrder
{
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<PlaceOrderCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public PlaceOrderCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser,
           ILogger<PlaceOrderCommandHandler> logger, IPublishEndpoint publishEndpointr)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _logger = logger;
            _publishEndpoint = publishEndpointr;
        }

        public async Task<PlaceOrderResponse> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ddddddd");

            var restaurant = _dbContext.Restaurants.AsNoTracking()
                .FirstOrDefault(r => r.Id == request.RestaurantId && r.IsActive);

            if (restaurant is null)
                throw new NotFoundException("Restaurant", request.RestaurantId);

            var order = new CustomerOrder(_currentUser.UserId, request.RestaurantId, restaurant.Name);

            var menuItemIds = request.Items.Select(c => c.MenuItemId).Distinct().ToList();

            var menuItems = await _dbContext.MenuItems
                .Where(x => x.RestaurantId == request.RestaurantId && menuItemIds.Contains(x.Id))
                .AsNoTracking().ToListAsync(cancellationToken);


            if (menuItems.Count != menuItemIds.Count)
                throw new NotFoundException("One or more menu items do not exist.");

            if (menuItems.Any(m => !m.IsAvailable))
                throw new ConflictException("One or more menu items are not available.");

            var menuItemDict = menuItems.ToDictionary(x => x.Id);

            foreach (var item in request.Items)
            {
                if (menuItemDict.TryGetValue(item.MenuItemId, out var menuItem))
                    order.AddOrderItem(item.Quantity, menuItem.Price, item.MenuItemId, menuItem.Name);
            }
            await _dbContext.CustomerOrders.AddAsync(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Order {OrderId} placed successfully. User: {UserId}, Restaurant: {RestaurantId}, Total: {TotalAmount}, ItemCount: {ItemCount}",
                order.Id, _currentUser.UserId, request.RestaurantId, order.TotalAmount, request.Items.Count);

            var orderPlaceEvent = OrderPlacedIntegrationEvent.
                CreateNew(order.Id, order.CustomerUserId, order.RestaurantId, order.TotalAmount);

            await _publishEndpoint.Publish(orderPlaceEvent, cancellationToken);

            return new PlaceOrderResponse(order.Id, order.Status, order.TotalAmount, order.CreatedAt);
        }
    }
}
