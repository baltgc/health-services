using System.Security.Claims;
using HealthServices.Application.DTOs;
using HealthServices.Domain.Entities;
using HealthServices.Domain.Enums;

namespace HealthServices.Application.Services;

public interface IAuthenticationService
{
    Task<AuthResultDto> HandleGoogleAuthenticationAsync(IEnumerable<Claim> claims);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto?> GetUserByGoogleIdAsync(string googleId);
    Task<AuthResultDto> UpdateUserRoleAsync(int userId, UserRole role);
    Task<bool> DeactivateUserAsync(int userId);
    Task<bool> ActivateUserAsync(int userId);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role);
    string GenerateJwtToken(User user);
    Task<bool> LogoutUserAsync(int userId);
}
