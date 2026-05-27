namespace OrderFlow.Application.Abstractions
{
    public interface IOrderNotificationService
    {
        /// <summary>
        /// Pushes a real-time WebSocket alert directly to a specific restaurant's active dashboard.
        /// </summary>
        // ۱. نوتیفیکیشن سفارش جدید به رستوران
        Task NotifyRestaurantOfNewOrderAsync(Guid restaurantId, Guid orderId, decimal totalAmount);

        // ۲. نوتیفیکیشن سفارش آماده شده به گروه پیک‌ها
        Task NotifyCouriersOfAvailableOrderAsync(Guid courierId, Guid orderId, string restaurantName, string deliveryAddress);

        // ۳. نوتیفیکیشن تغییرات وضعیت سفارش به مشتری (مثلاً پیک در راه است)
        Task NotifyCustomerOfOrderStatusAsync(Guid customerId, Guid orderId, string status, string message);
    }
}

