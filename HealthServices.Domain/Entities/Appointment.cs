using System.ComponentModel.DataAnnotations;

namespace HealthServices.Domain.Entities;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan AppointmentTime { get; set; }

    [Required]
    public int DurationMinutes { get; set; } = 30;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, NoShow

    [MaxLength(100)]
    public string AppointmentType { get; set; } = string.Empty; // Consultation, Follow-up, Emergency, etc.

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string DoctorNotes { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Prescription { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Diagnosis { get; set; } = string.Empty;

    public decimal? ConsultationFee { get; set; }

    [MaxLength(50)]
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Cancelled

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public PatientProfile Patient { get; set; } = null!;
    public DoctorProfile Doctor { get; set; } = null!;

    // Computed properties
    public DateTime AppointmentDateTime => AppointmentDate.Add(AppointmentTime);
    public DateTime AppointmentEndTime => AppointmentDateTime.AddMinutes(DurationMinutes);
    public bool IsExpired => DateTime.Now > AppointmentEndTime && Status == "Scheduled";
}
