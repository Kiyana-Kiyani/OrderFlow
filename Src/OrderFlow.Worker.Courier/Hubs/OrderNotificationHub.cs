using Microsoft.AspNetCore.SignalR;

namespace OrderFlow.Worker.Courier.Hubs
{
    /// <summary>
    /// This is an empty infrastructure marker class. It must match the name of the 
    /// Hub inside your Web API project so the Redis Pub/Sub backplane can route 
    /// messages to the correct websocket channels across application boundaries.
    /// </summary>
    public class OrderNotificationHub : Hub
    {
    }
}
