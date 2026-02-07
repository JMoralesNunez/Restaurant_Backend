using RestaurantBackend.Application.DTOs;
using RestaurantBackend.Application.Interfaces;
using RestaurantBackend.Domain.Entities;
using RestaurantBackend.Domain.Enums;
using RestaurantBackend.Domain.Interfaces;

namespace RestaurantBackend.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderNotificationService _notificationService;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IOrderNotificationService notificationService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(int? userId, bool isAdmin)
    {
        var orders = isAdmin
            ? await _orderRepository.GetAllAsync()
            : await _orderRepository.GetByUserIdAsync(userId!.Value);

        return orders.Select(MapToOrderDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id, int? userId, bool isAdmin)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id);
        if (order == null)
        {
            return null;
        }

        // Check access rights
        if (!isAdmin && order.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have access to this order");
        }

        return MapToOrderDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, int userId)
    {
        // Validate at least one item
        if (createOrderDto.Items == null || !createOrderDto.Items.Any())
        {
            throw new ArgumentException("Order must have at least one item");
        }

        // Create order
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.PENDING,
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>()
        };

        // Process each item
        foreach (var itemDto in createOrderDto.Items)
        {
            // Validate quantity
            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }

            // Get product
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {itemDto.ProductId} not found");
            }

            // Validate product is active
            if (!product.IsActive)
            {
                throw new InvalidOperationException($"Product {product.Name} is not available");
            }

            // Create order item with historical price
            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                Price = product.Price // Copy current price
            };

            order.OrderItems.Add(orderItem);
        }

        // Calculate total
        order.CalculateTotal();

        // Save order
        var createdOrder = await _orderRepository.CreateAsync(order);

        // Reload with details
        var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(createdOrder.Id);

        // Notify admins
        await _notificationService.NotifyNewOrderAsync(createdOrder.Id);

        return MapToOrderDto(orderWithDetails!);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        var order = await _orderRepository.UpdateStatusAsync(id, status);

        // Notify user and admins
        await _notificationService.NotifyOrderStatusChangedAsync(order.Id, order.UserId, status.ToString());

        var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(order.Id);
        return MapToOrderDto(orderWithDetails!);
    }

    public async Task DeleteOrderAsync(int id, int userId, bool isAdmin)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        // Check permissions
        if (!isAdmin)
        {
            // User can only cancel their own orders
            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have access to this order");
            }

            // User can only cancel pending orders
            if (order.Status != OrderStatus.PENDING)
            {
                throw new InvalidOperationException("Only pending orders can be cancelled");
            }
        }

        // Change status to CANCELLED instead of deleting
        await _orderRepository.UpdateStatusAsync(id, OrderStatus.CANCELLED);

        // Notify user and admins
        await _notificationService.NotifyOrderStatusChangedAsync(id, order.UserId, "CANCELLED");
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = order.User?.Name ?? string.Empty,
            Status = order.Status,
            Total = order.Total,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        };
    }
}
