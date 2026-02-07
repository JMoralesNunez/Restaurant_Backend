using Microsoft.AspNetCore.SignalR;
using RestaurantBackend.Application.Interfaces;
using RestaurantBackend.API.Hubs;

namespace RestaurantBackend.API.Services;

public class OrderNotificationService : IOrderNotificationService
{
    private readonly IHubContext<OrderHub> _hubContext;

    public OrderNotificationService(IHubContext<OrderHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyOrderStatusChangedAsync(int orderId, int userId, string status)
    {
        // Notify the specific user
        await _hubContext.Clients.Group($"User_{userId}")
            .SendAsync("OrderStatusChanged", new { orderId, status });

        // Notify admins to update dashboard
        await _hubContext.Clients.Group("Admins")
            .SendAsync("DashboardUpdated");
    }

    public async Task NotifyNewOrderAsync(int orderId)
    {
        // Notify admins there's a new order
        await _hubContext.Clients.Group("Admins")
            .SendAsync("NewOrderReceived", new { orderId });
    }
}
