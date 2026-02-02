using RestaurantBackend.Domain.Entities;
using RestaurantBackend.Domain.Enums;

namespace RestaurantBackend.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task DeleteAsync(int id);
    Task<Order> UpdateStatusAsync(int id, OrderStatus status);
}
