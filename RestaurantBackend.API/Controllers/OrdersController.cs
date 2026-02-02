using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantBackend.Application.DTOs;
using RestaurantBackend.Application.Interfaces;
using RestaurantBackend.Domain.Enums;
using System.Security.Claims;

namespace RestaurantBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin() => User.IsInRole("ADMIN");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var isAdmin = IsAdmin();
        var userId = isAdmin ? null : (int?)GetCurrentUserId();
        var orders = await _orderService.GetAllOrdersAsync(userId, isAdmin);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id, GetCurrentUserId(), IsAdmin());
            if (order == null) return NotFound();
            return Ok(order);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [Authorize(Roles = "USER")]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto createOrderDto)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(createOrderDto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusDto statusDto)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, statusDto.Status);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _orderService.DeleteOrderAsync(id, GetCurrentUserId(), IsAdmin());
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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
