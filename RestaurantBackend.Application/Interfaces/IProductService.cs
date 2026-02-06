using RestaurantBackend.Application.DTOs;

namespace RestaurantBackend.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(bool isAdmin);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    Task<ProductDto> UpdateProductImageAsync(int id, string imageUrl, string imagePublicId);
    Task DeleteProductAsync(int id);
}
