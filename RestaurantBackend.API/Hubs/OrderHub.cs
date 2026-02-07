using Microsoft.AspNetCore.SignalR;

namespace RestaurantBackend.API.Hubs;

public class OrderHub : Hub
{
    public async Task JoinUserGroup(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
    }

    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
    }
}
