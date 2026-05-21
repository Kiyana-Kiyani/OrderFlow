using Microsoft.AspNetCore.SignalR;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Infrastructure.Notifications
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderNotificationService(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyRestaurantOfNewOrderAsync(Guid restaurantId, Guid orderId, decimal totalAmount)
        {
            // Target only the clients connected inside the specific restaurant's group room
            await _hubContext.Clients
                .Group(restaurantId.ToString())
                .SendAsync("ReceiveNewOrder", new
                {
                    OrderId = orderId,
                    TotalAmount = totalAmount
                });
        }
    }
}