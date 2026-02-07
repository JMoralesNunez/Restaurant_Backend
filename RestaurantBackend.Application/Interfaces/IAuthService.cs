using RestaurantBackend.Application.DTOs;

namespace RestaurantBackend.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(CreateUserDto createUserDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserDto> PromoteToAdminAsync(int userId);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task DeleteUserAsync(int id);
}
