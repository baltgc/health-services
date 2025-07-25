namespace HealthServices.Application.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash, string salt);
    string GenerateSalt();
}
