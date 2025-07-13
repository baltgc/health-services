using HealthServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthServices.Infrastructure.DbContexts;

public class HealthServicesDbContext : DbContext
{
    public HealthServicesDbContext(DbContextOptions<HealthServicesDbContext> options)
        : base(options) { }

    // DbSets
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PatientProfile> PatientProfiles { get; set; } = null!;
    public DbSet<DoctorProfile> DoctorProfiles { get; set; } = null!;
    public DbSet<Specialty> Specialties { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<MedicalHistory> MedicalHistories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure one-to-one relationships
            entity
                .HasOne(u => u.PatientProfile)
                .WithOne(p => p.User)
                .HasForeignKey<PatientProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(u => u.DoctorProfile)
                .WithOne(d => d.User)
                .HasForeignKey<DoctorProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PatientProfile entity
        modelBuilder.Entity<PatientProfile>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.PatientId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.PatientId).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure one-to-many relationships
            entity
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(p => p.MedicalHistories)
                .WithOne(m => m.Patient)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure DoctorProfile entity
        modelBuilder.Entity<DoctorProfile>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.DoctorId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.DoctorId).IsUnique();
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationships
            entity
                .HasOne(d => d.Specialty)
                .WithMany(s => s.DoctorProfiles)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasMany(d => d.Appointments)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(d => d.MedicalHistories)
                .WithOne(m => m.Doctor)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Specialty entity
        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure Appointment entity
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConsultationFee).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure composite index for efficient querying
            entity.HasIndex(e => new { e.PatientId, e.AppointmentDate });
            entity.HasIndex(e => new { e.DoctorId, e.AppointmentDate });
            entity.HasIndex(e => e.Status);
        });

        // Configure MedicalHistory entity
        modelBuilder.Entity<MedicalHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChiefComplaint).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship with Appointment (optional)
            entity
                .HasOne(m => m.Appointment)
                .WithMany()
                .HasForeignKey(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes for efficient querying
            entity.HasIndex(e => new { e.PatientId, e.VisitDate });
            entity.HasIndex(e => new { e.DoctorId, e.VisitDate });
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Specialties
        modelBuilder
            .Entity<Specialty>()
            .HasData(
                new Specialty
                {
                    Id = 1,
                    Name = "Cardiology",
                    Description = "Heart and cardiovascular system",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 2,
                    Name = "Dermatology",
                    Description = "Skin, hair, and nail disorders",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 3,
                    Name = "Neurology",
                    Description = "Nervous system disorders",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 4,
                    Name = "Orthopedics",
                    Description = "Musculoskeletal system",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 5,
                    Name = "Pediatrics",
                    Description = "Child healthcare",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 6,
                    Name = "Psychiatry",
                    Description = "Mental health disorders",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 7,
                    Name = "General Practice",
                    Description = "Primary healthcare",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 8,
                    Name = "Oncology",
                    Description = "Cancer treatment",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 9,
                    Name = "Gynecology",
                    Description = "Women's health",
                    CreatedAt = DateTime.UtcNow,
                },
                new Specialty
                {
                    Id = 10,
                    Name = "Ophthalmology",
                    Description = "Eye and vision care",
                    CreatedAt = DateTime.UtcNow,
                }
            );
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is User
                || e.Entity is PatientProfile
                || e.Entity is DoctorProfile
                || e.Entity is Specialty
                || e.Entity is Appointment
                || e.Entity is MedicalHistory
            );

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is User user)
                    user.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is PatientProfile patient)
                    patient.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is DoctorProfile doctor)
                    doctor.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is Specialty specialty)
                    specialty.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is Appointment appointment)
                    appointment.UpdatedAt = DateTime.UtcNow;
                else if (entry.Entity is MedicalHistory medicalHistory)
                    medicalHistory.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
