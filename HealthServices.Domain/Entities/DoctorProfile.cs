using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthServices.Domain.Entities;

public class DoctorProfile
{
    [Key]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string DoctorId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    public int SpecialtyId { get; set; }

    [MaxLength(200)]
    public string Education { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Experience { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Certifications { get; set; } = string.Empty;

    [MaxLength(500)]
    public string WorkingHours { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ConsultationFee { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ClinicAddress { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ClinicPhone { get; set; } = string.Empty;

    [Required]
    public bool IsAvailable { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Specialty Specialty { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = [];
}
