using System.ComponentModel.DataAnnotations;

namespace HealthServices.Domain.Entities;

public class MedicalHistory
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    public int? AppointmentId { get; set; }

    [Required]
    public DateTime VisitDate { get; set; }

    [Required]
    [MaxLength(500)]
    public string ChiefComplaint { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string PresentIllness { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string PastMedicalHistory { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string FamilyHistory { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string SocialHistory { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string PhysicalExamination { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string VitalSigns { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Investigations { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Diagnosis { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Treatment { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Prescription { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Recommendations { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string FollowUpInstructions { get; set; } = string.Empty;

    public DateTime? NextVisitDate { get; set; }

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public PatientProfile Patient { get; set; } = null!;
    public DoctorProfile Doctor { get; set; } = null!;
    public Appointment? Appointment { get; set; }
}
