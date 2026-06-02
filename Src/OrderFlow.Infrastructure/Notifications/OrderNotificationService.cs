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

        // ارسال به رستوران خاص
        public async Task NotifyRestaurantOfNewOrderAsync(Guid restaurantId, Guid orderId, decimal totalAmount)
        {
            await _hubContext.Clients
                .Group($"Restaurant_{restaurantId}")
                .SendAsync("ReceiveNewOrder", new { OrderId = orderId, TotalAmount = totalAmount });
        }

        //// ارسال به همه پیک‌ها
        //public async Task NotifyCouriersOfAvailableOrderAsync(Guid courierId, Guid orderId, string restaurantName, string deliveryAddress)
        //{
        //    await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", new { Message = "تست موفقیت آمیز" });
        //}

        // ارسال به مشتری خاص
        public async Task NotifyCustomerOfOrderStatusAsync(Guid customerId, Guid orderId, string status, string message)
        {
            //  await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", new { OrderId = orderId, Status = status, Message = message });

            await _hubContext.Clients
                .Group($"Customer_{customerId}")
                .SendAsync("ReceiveStatusUpdate", new { OrderId = orderId, Status = status, Message = message });
        }
    }
}