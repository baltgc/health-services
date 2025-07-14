using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthServices.Application.DTOs;
using HealthServices.Application.Services;
using HealthServices.Domain.Entities;
using HealthServices.Domain.Enums;
using HealthServices.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HealthServices.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HealthServicesDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IPasswordService _passwordService;

    public AuthenticationService(
        HealthServicesDbContext context,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger,
        IPasswordService passwordService
    )
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _passwordService = passwordService;
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

    // Native authentication methods
    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Validate request
            if (
                string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password)
            )
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Email and password are required",
                };
            }

            if (request.Password != request.ConfirmPassword)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Passwords do not match",
                };
            }

            if (request.Password.Length < 8)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Password must be at least 8 characters long",
                };
            }

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == request.Email
            );
            if (existingUser != null)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "User with this email already exists",
                };
            }

            // Create new user
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender ?? string.Empty,
                Address = request.Address ?? string.Empty,
                City = request.City ?? string.Empty,
                State = request.State ?? string.Empty,
                ZipCode = request.ZipCode ?? string.Empty,
                Country = request.Country ?? string.Empty,
                PasswordHash = _passwordService.HashPassword(request.Password),
                PasswordSalt = _passwordService.GenerateSalt(),
                Role = UserRole.Patient,
                IsEmailVerified = false, // Will need email verification for native auth
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(user);
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
            _logger.LogError(ex, "Error during user registration");
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred during registration",
            };
        }
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password)
            )
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Email and password are required",
                };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid email or password",
                };
            }

            if (!user.IsActive)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Account is deactivated",
                };
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid email or password",
                };
            }

            if (
                !_passwordService.VerifyPassword(
                    request.Password,
                    user.PasswordHash,
                    user.PasswordSalt ?? string.Empty
                )
            )
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid email or password",
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
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
            _logger.LogError(ex, "Error during login");
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred during login",
            };
        }
    }

    public async Task<AuthResultDto> ChangePasswordAsync(
        int userId,
        ChangePasswordRequestDto request
    )
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new AuthResultDto { IsSuccess = false, ErrorMessage = "User not found" };
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "User does not have a password set",
                };
            }

            if (
                !_passwordService.VerifyPassword(
                    request.CurrentPassword,
                    user.PasswordHash,
                    user.PasswordSalt ?? string.Empty
                )
            )
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Current password is incorrect",
                };
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "New passwords do not match",
                };
            }

            if (request.NewPassword.Length < 8)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "New password must be at least 8 characters long",
                };
            }

            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.PasswordSalt = _passwordService.GenerateSalt();
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
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while changing password",
            };
        }
    }

    public async Task<AuthResultDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                // Don't reveal if user exists or not for security
                return new AuthResultDto { IsSuccess = true, LoginResponse = null };
            }

            // TODO: Implement email service to send password reset email
            // For now, just return success
            _logger.LogInformation("Password reset requested for user {Email}", request.Email);

            return new AuthResultDto { IsSuccess = true, LoginResponse = null };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for {Email}", request.Email);
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while processing the request",
            };
        }
    }

    public async Task<AuthResultDto> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid reset token or email",
                };
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "New passwords do not match",
                };
            }

            if (request.NewPassword.Length < 8)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "New password must be at least 8 characters long",
                };
            }

            // TODO: Validate reset token
            // For now, just update the password
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.PasswordSalt = _passwordService.GenerateSalt();
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
            _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while resetting password",
            };
        }
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return false;
            }

            return _passwordService.VerifyPassword(
                password,
                user.PasswordHash,
                user.PasswordSalt ?? string.Empty
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for {Email}", email);
            return false;
        }
    }

    public async Task<AuthResultDto> RefreshTokenAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new AuthResultDto { IsSuccess = false, ErrorMessage = "User not found" };
            }

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
            _logger.LogError(ex, "Error refreshing token for user {UserId}", userId);
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while refreshing the token",
            };
        }
    }

    public async Task<AuthResultDto> GenerateTestTokenAsync(UserDto userDto)
    {
        try
        {
            // Create a temporary user entity for token generation
            var user = new User
            {
                Id = userDto.Id,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Role = Enum.Parse<UserRole>(userDto.Role),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpiryMinutes());

            return new AuthResultDto
            {
                IsSuccess = true,
                LoginResponse = new LoginResponseDto
                {
                    Token = token,
                    User = userDto,
                    ExpiresAt = expiresAt,
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating test token for user {Email}", userDto.Email);
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while generating the test token",
            };
        }
    }
}
