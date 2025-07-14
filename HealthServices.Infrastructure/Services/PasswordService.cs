using System.Security.Cryptography;
using System.Text;
using HealthServices.Application.Services;

namespace HealthServices.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        // Use BCrypt for secure password hashing
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            // BCrypt stores the salt as part of the hash, so we don't need the separate salt parameter
            // The salt parameter is kept for compatibility but not used with BCrypt
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateSalt()
    {
        // Generate a random salt using cryptographically secure random number generator
        var saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }
}
