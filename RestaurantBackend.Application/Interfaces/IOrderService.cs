using RestaurantBackend.Application.DTOs;
using RestaurantBackend.Domain.Enums;

namespace RestaurantBackend.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(int? userId, bool isAdmin);
    Task<OrderDto?> GetOrderByIdAsync(int id, int? userId, bool isAdmin);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, int userId);
    Task<OrderDto> UpdateOrderStatusAsync(int id, OrderStatus status);
    Task DeleteOrderAsync(int id, int userId, bool isAdmin);
}
