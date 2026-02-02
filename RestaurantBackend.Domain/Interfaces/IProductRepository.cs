using RestaurantBackend.Domain.Entities;

namespace RestaurantBackend.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<bool> HasOrdersAsync(int productId);
}
