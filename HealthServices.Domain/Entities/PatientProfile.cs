using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthServices.Domain.Entities;

public class PatientProfile
{
    [Key]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string PatientId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string EmergencyContactPhone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EmergencyContactRelationship { get; set; } = string.Empty;

    [MaxLength(100)]
    public string InsuranceProvider { get; set; } = string.Empty;

    [MaxLength(50)]
    public string InsurancePolicyNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string BloodType { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Allergies { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string CurrentMedications { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string ChronicConditions { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = [];
}
