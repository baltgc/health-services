namespace HealthServices.Application.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AuthResultDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public LoginResponseDto? LoginResponse { get; set; }
}

public class RefreshTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
}

public class UpdateUserRoleRequestDto
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
