namespace OrderFlow.Worker.Courier.Abstractions
{
    public interface IPushNotificationService
    {
        Task SendPushAsync(string userId, string title, object payload);
    }
}
