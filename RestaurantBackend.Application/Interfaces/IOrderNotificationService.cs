namespace RestaurantBackend.Application.Interfaces;

public interface IOrderNotificationService
{
    Task NotifyOrderStatusChangedAsync(int orderId, int userId, string status);
    Task NotifyNewOrderAsync(int orderId);
}
