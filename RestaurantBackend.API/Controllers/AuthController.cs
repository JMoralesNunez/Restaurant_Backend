using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantBackend.Application.DTOs;
using RestaurantBackend.Application.Interfaces;

namespace RestaurantBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(CreateUserDto createUserDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(createUserDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        try
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPatch("promote/{userId}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<UserDto>> Promote(int userId)
    {
        try
        {
            var result = await _authService.PromoteToAdminAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("users")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
    {
        try
        {
            var result = await _authService.UpdateUserAsync(id, updateUserDto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await _authService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
