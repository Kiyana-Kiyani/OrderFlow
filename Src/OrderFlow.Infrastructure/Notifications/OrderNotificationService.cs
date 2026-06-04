using Microsoft.AspNetCore.SignalR;
using OrderFlow.Application.Abstractions;
using OrderFlow.Contracts.Hubs;

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
            await _hubContext.Clients
                .Group($"Restaurant_{restaurantId}")
                .SendAsync("ReceiveNewOrder", new { OrderId = orderId, TotalAmount = totalAmount });
        }

        public async Task NotifyCustomerOfOrderStatusAsync(Guid customerId, Guid orderId, string status, string message)
        {
            await _hubContext.Clients
                .Group($"Customer_{customerId}")
                .SendAsync("ReceiveStatusUpdate", new { OrderId = orderId, Status = status, Message = message });
        }
    }
}