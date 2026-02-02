using RestaurantBackend.Domain.Enums;

namespace RestaurantBackend.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Method to calculate total
    public void CalculateTotal()
    {
        Total = OrderItems.Sum(item => item.Quantity * item.Price);
    }
}
