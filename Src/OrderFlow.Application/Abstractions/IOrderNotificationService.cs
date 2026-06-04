namespace OrderFlow.Application.Abstractions
{
    public interface IOrderNotificationService
    {
        Task NotifyRestaurantOfNewOrderAsync(Guid restaurantId, Guid orderId, decimal totalAmount);
        Task NotifyCustomerOfOrderStatusAsync(Guid customerId, Guid orderId, string status, string message);
    }
}

