namespace OrderFlow.Application.Abstractions
{
    public interface IOrderNotificationService
    {
        /// <summary>
        /// Pushes a real-time WebSocket alert directly to a specific restaurant's active dashboard.
        /// </summary>
        Task NotifyRestaurantOfNewOrderAsync(Guid restaurantId, Guid orderId, decimal totalAmount);
    }
}

