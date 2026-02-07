namespace RestaurantBackend.Domain.Enums;

public enum UserRole
{
    USER,
    ADMIN
}

public enum OrderStatus
{
    PENDING,
    PREPARING,
    DELIVERED,
    CANCELLED
}

public enum ProductCategory
{
    Food,
    Drink,
    Dessert
}
