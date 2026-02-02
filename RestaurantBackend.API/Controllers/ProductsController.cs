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

    public ProductsController(IProductService productService)
    {
        _productService = productService;
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
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto createProductDto)
    {
        var product = await _productService.CreateProductAsync(createProductDto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ProductDto>> Update(int id, UpdateProductDto updateProductDto)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(id, updateProductDto);
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
