using RestaurantBackend.Application.DTOs;
using RestaurantBackend.Application.Interfaces;
using RestaurantBackend.Domain.Entities;
using RestaurantBackend.Domain.Interfaces;

namespace RestaurantBackend.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public ProductService(IProductRepository productRepository, ICloudinaryService cloudinaryService)
    {
        _productRepository = productRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(bool isAdmin)
    {
        var products = isAdmin
            ? await _productRepository.GetAllAsync()
            : await _productRepository.GetActiveProductsAsync();

        return products.Select(MapToProductDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : MapToProductDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        // Validate price and stock
        if (createProductDto.Price < 0)
        {
            throw new ArgumentException("Price must be greater than or equal to 0");
        }

        if (createProductDto.Stock < 0)
        {
            throw new ArgumentException("Stock must be greater than or equal to 0");
        }

        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            Stock = createProductDto.Stock,
            IsActive = createProductDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var createdProduct = await _productRepository.CreateAsync(product);
        return MapToProductDto(createdProduct);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found");
        }

        // Update only provided fields
        if (updateProductDto.Name != null)
            product.Name = updateProductDto.Name;

        if (updateProductDto.Description != null)
            product.Description = updateProductDto.Description;

        if (updateProductDto.Price.HasValue)
        {
            if (updateProductDto.Price.Value < 0)
            {
                throw new ArgumentException("Price must be greater than or equal to 0");
            }
            product.Price = updateProductDto.Price.Value;
        }

        if (updateProductDto.Stock.HasValue)
        {
            if (updateProductDto.Stock.Value < 0)
            {
                throw new ArgumentException("Stock must be greater than or equal to 0");
            }
            product.Stock = updateProductDto.Stock.Value;
        }

        if (updateProductDto.IsActive.HasValue)
            product.IsActive = updateProductDto.IsActive.Value;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return MapToProductDto(updatedProduct);
    }

    public async Task<ProductDto> UpdateProductImageAsync(int id, string imageUrl, string imagePublicId)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found");
        }

        // Delete old image if exists
        if (!string.IsNullOrEmpty(product.ImagePublicId))
        {
            await _cloudinaryService.DeleteImageAsync(product.ImagePublicId);
        }

        product.ImageUrl = imageUrl;
        product.ImagePublicId = imagePublicId;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return MapToProductDto(updatedProduct);
    }

    public async Task DeleteProductAsync(int id)
    {
        // Check if product has orders
        if (await _productRepository.HasOrdersAsync(id))
        {
            throw new InvalidOperationException("Cannot delete product with existing orders");
        }

        // Delete image from Cloudinary if exists
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null && !string.IsNullOrEmpty(product.ImagePublicId))
        {
            await _cloudinaryService.DeleteImageAsync(product.ImagePublicId);
        }

        await _productRepository.DeleteAsync(id);
    }

    private static ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt
        };
    }
}
