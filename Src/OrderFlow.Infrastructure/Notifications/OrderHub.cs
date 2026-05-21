using Microsoft.AspNetCore.SignalR;

namespace OrderFlow.Infrastructure.Notifications
{
    public class OrderHub : Hub
    {
        // Allows a restaurant dashboard client to join their specific group room
        public async Task JoinRestaurantGroup(string restaurantId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, restaurantId);
        }

        public async Task LeaveRestaurantGroup(string restaurantId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, restaurantId);
        }
    }
}