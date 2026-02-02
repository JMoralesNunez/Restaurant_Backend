using RestaurantBackend.Application.DTOs;

namespace RestaurantBackend.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(CreateUserDto createUserDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
}
