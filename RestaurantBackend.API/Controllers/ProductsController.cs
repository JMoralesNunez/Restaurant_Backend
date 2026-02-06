using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantBackend.Application.DTOs;
using RestaurantBackend.Application.Interfaces;
using System.Security.Claims;

namespace RestaurantBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICloudinaryService _cloudinaryService;

    public ProductsController(IProductService productService, ICloudinaryService cloudinaryService)
    {
        _productService = productService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var isAdmin = User.IsInRole("ADMIN");
        var products = await _productService.GetAllProductsAsync(isAdmin);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        
        // Hide inactive products from non-admins
        if (!product.IsActive && !User.IsInRole("ADMIN"))
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ProductDto>> Create([FromForm] CreateProductDto createProductDto, IFormFile? image)
    {
        try
        {
            // Create product first
            var product = await _productService.CreateProductAsync(createProductDto);

            // Upload image if provided
            if (image != null)
            {
                var validationError = ValidateImageFile(image);
                if (validationError != null)
                {
                    return BadRequest(new { message = validationError });
                }

                using var stream = image.OpenReadStream();
                var (imageUrl, publicId) = await _cloudinaryService.UploadImageAsync(stream, image.FileName);
                product = await _productService.UpdateProductImageAsync(product.Id, imageUrl, publicId);
            }

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromForm] UpdateProductDto updateProductDto, IFormFile? image)
    {
        try
        {
            // Update product data
            var product = await _productService.UpdateProductAsync(id, updateProductDto);

            // Upload new image if provided
            if (image != null)
            {
                var validationError = ValidateImageFile(image);
                if (validationError != null)
                {
                    return BadRequest(new { message = validationError });
                }

                using var stream = image.OpenReadStream();
                var (imageUrl, publicId) = await _cloudinaryService.UploadImageAsync(stream, image.FileName);
                product = await _productService.UpdateProductImageAsync(product.Id, imageUrl, publicId);
            }

            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? ValidateImageFile(IFormFile image)
    {
        // Validate file
        if (image.Length == 0)
        {
            return "Image file is empty";
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return "Invalid file type. Allowed types: jpg, jpeg, png, webp";
        }

        // Validate file size (5MB max)
        const int maxFileSize = 5 * 1024 * 1024;
        if (image.Length > maxFileSize)
        {
            return "File size exceeds 5MB limit";
        }

        return null;
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
