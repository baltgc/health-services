using HealthServices.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthServices.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly HealthServices.Application.Services.IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        HealthServices.Application.Services.IAuthenticationService authService,
        ILogger<AuthController> logger
    )
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Initiate Google OAuth login
    /// </summary>
    [HttpGet("login/google")]
    public IActionResult LoginWithGoogle()
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handle Google OAuth callback
    /// </summary>
    [HttpGet("callback/google")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = "Google authentication failed" });
        }

        var claims = result.Principal?.Claims;
        if (claims == null)
        {
            return BadRequest(new { error = "No claims received from Google" });
        }

        var authResult = await _authService.HandleGoogleAuthenticationAsync(claims);

        if (!authResult.IsSuccess)
        {
            return BadRequest(new { error = authResult.ErrorMessage });
        }

        return Ok(authResult.LoginResponse);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var success = await _authService.LogoutUserAsync(userId.Value);
        if (!success)
        {
            return BadRequest(new { error = "Logout failed" });
        }

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        // Delegate token refresh to the service layer
        var refreshResult = await _authService.RefreshTokenAsync(userId.Value);
        if (!refreshResult.IsSuccess)
        {
            return BadRequest(new { error = refreshResult.ErrorMessage });
        }

        return Ok(refreshResult.LoginResponse);
    }

    /// <summary>
    /// Register a new user with native authentication
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.LoginResponse);
    }

    /// <summary>
    /// Login with native authentication
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.LoginResponse);
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(userId.Value, request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.LoginResponse);
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.ForgotPasswordAsync(request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.ResetPasswordAsync(request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.LoginResponse);
    }

#if DEBUG
    /// <summary>
    /// Generate test JWT token for development/testing (DEBUG only)
    /// </summary>
    [HttpPost("test-token")]
    public async Task<IActionResult> GenerateTestToken([FromBody] TestTokenRequest request)
    {
        // This endpoint is only available in DEBUG mode for testing
        var testUser = new UserDto
        {
            Id = request.UserId ?? 1,
            Email = request.Email ?? "test@example.com",
            FirstName = request.FirstName ?? "Test",
            LastName = request.LastName ?? "User",
            Role = request.Role ?? "Patient",
        };

        var testResult = await _authService.GenerateTestTokenAsync(testUser);
        if (!testResult.IsSuccess)
        {
            return BadRequest(new { error = testResult.ErrorMessage });
        }

        return Ok(testResult.LoginResponse);
    }

    public class TestTokenRequest
    {
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; } = "Patient";
    }
#endif

    private int? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
