using Microsoft.AspNetCore.SignalR;

namespace OrderFlow.Worker.Courier.Hubs
{
    /// <summary>
    /// This is an empty infrastructure marker class. It must match the name of the 
    /// Hub inside your Web API project so the Redis Pub/Sub backplane can route 
    /// messages to the correct websocket channels across application boundaries.
    /// </summary>
    ///  این یک کلاس خالی (Stub) است تا MassTransit بتواند ساختار گروه بندی هاب API را پروکسی کند

    public class OrderHub : Hub
    {
    }
}
