using RestaurantBackend.Application.DTOs;
using RestaurantBackend.Application.Interfaces;
using RestaurantBackend.Domain.Entities;
using RestaurantBackend.Domain.Enums;
using RestaurantBackend.Domain.Interfaces;

namespace RestaurantBackend.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(CreateUserDto createUserDto)
    {
        // Validate email uniqueness
        if (await _userRepository.EmailExistsAsync(createUserDto.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Create user entity
        var user = new User
        {
            Name = createUserDto.Name,
            Email = createUserDto.Email,
            PasswordHash = _passwordHasher.HashPassword(createUserDto.Password),
            Role = UserRole.USER,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Save to database
        var createdUser = await _userRepository.CreateAsync(user);

        // Generate JWT token
        var token = _jwtService.GenerateToken(
            createdUser.Id,
            createdUser.Email,
            createdUser.Role.ToString()
        );

        return new AuthResponseDto
        {
            Token = token,
            User = MapToUserDto(createdUser)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(
            user.Id,
            user.Email,
            user.Role.ToString()
        );

        return new AuthResponseDto
        {
            Token = token,
            User = MapToUserDto(user)
        };
    }

    public async Task<UserDto> PromoteToAdminAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        user.Role = UserRole.ADMIN;
        var updatedUser = await _userRepository.UpdateAsync(user);

        return MapToUserDto(updatedUser);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        if (updateUserDto.Name != null) user.Name = updateUserDto.Name;
        if (updateUserDto.Email != null) user.Email = updateUserDto.Email;
        if (updateUserDto.Role.HasValue) user.Role = updateUserDto.Role.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        return MapToUserDto(updatedUser);
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        await _userRepository.DeleteAsync(id);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
