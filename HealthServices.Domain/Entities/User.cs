using System.ComponentModel.DataAnnotations;
using HealthServices.Domain.Enums;

namespace HealthServices.Domain.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string Gender { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(50)]
    public string State { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ZipCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Country { get; set; } = string.Empty;

    // Authentication properties
    [MaxLength(100)]
    public string GoogleId { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? GoogleAccessToken { get; set; }

    [MaxLength(1000)]
    public string? GoogleRefreshToken { get; set; }

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    // Native authentication properties
    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    [MaxLength(128)]
    public string? PasswordSalt { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Patient;

    [Required]
    public bool IsEmailVerified { get; set; } = true; // Google emails are verified by default

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public PatientProfile? PatientProfile { get; set; }
    public DoctorProfile? DoctorProfile { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public int? Age =>
        DateOfBirth.HasValue
            ? DateTime.Now.Year
                - DateOfBirth.Value.Year
                - (DateTime.Now.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
            : null;
}
