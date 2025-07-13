using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthServices.Application.DTOs;
using HealthServices.Domain.Entities;
using HealthServices.Domain.Enums;
using HealthServices.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HealthServices.Infrastructure.Services;

public class AuthenticationService : HealthServices.Application.Services.IAuthenticationService
{
    private readonly HealthServicesDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        HealthServicesDbContext context,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger
    )
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResultDto> HandleGoogleAuthenticationAsync(IEnumerable<Claim> claims)
    {
        try
        {
            var googleId = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var firstName = claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
            var profilePicture = claims.FirstOrDefault(x => x.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Required user information not received from Google",
                };
            }

            // Find or create user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    GoogleId = googleId,
                    Email = email,
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    ProfilePictureUrl = profilePicture,
                    Role = UserRole.Patient,
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.Users.Add(user);
            }
            else
            {
                // Update existing user
                user.Email = email;
                user.FirstName = firstName ?? user.FirstName;
                user.LastName = lastName ?? user.LastName;
                user.ProfilePictureUrl = profilePicture ?? user.ProfilePictureUrl;
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes());

            return new AuthResultDto
            {
                IsSuccess = true,
                LoginResponse = new LoginResponseDto
                {
                    Token = token,
                    User = MapUserToDto(user),
                    ExpiresAt = expiresAt,
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred during authentication",
            };
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user != null ? MapUserToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user != null ? MapUserToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByGoogleIdAsync(string googleId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        return user != null ? MapUserToDto(user) : null;
    }

    public async Task<AuthResultDto> UpdateUserRoleAsync(int userId, UserRole role)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new AuthResultDto { IsSuccess = false, ErrorMessage = "User not found" };
            }

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResultDto
            {
                IsSuccess = true,
                LoginResponse = new LoginResponseDto
                {
                    Token = GenerateJwtToken(user),
                    User = MapUserToDto(user),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes()),
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for user {UserId}", userId);
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while updating user role",
            };
        }
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ActivateUserAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return false;

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapUserToDto);
    }

    public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role)
    {
        var users = await _context.Users.Where(u => u.Role == role).ToListAsync();
        return users.Select(MapUserToDto);
    }

    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("GoogleId", user.GoogleId),
        };

        var token = new JwtSecurityToken(
            issuer: GetJwtIssuer(),
            audience: GetJwtAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> LogoutUserAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return false;

            // Clear Google tokens
            user.GoogleAccessToken = null;
            user.GoogleRefreshToken = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out user {UserId}", userId);
            return false;
        }
    }

    private UserDto MapUserToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
        };
    }

    private string GetJwtKey()
    {
        return _configuration["Authentication:Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key not configured");
    }

    private string GetJwtIssuer()
    {
        return _configuration["Authentication:Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer not configured");
    }

    private string GetJwtAudience()
    {
        return _configuration["Authentication:Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience not configured");
    }

    private int GetJwtExpiryMinutes()
    {
        return int.Parse(_configuration["Authentication:Jwt:ExpiryInMinutes"] ?? "60");
    }
}
