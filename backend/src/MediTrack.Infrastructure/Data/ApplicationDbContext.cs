using MediTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<Medicine> Medicines { get; set; } = null!;
    public DbSet<MedicineUnit> MedicineUnits { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<ScheduleTime> ScheduleTimes { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();

            // Relationships
            entity.HasOne(e => e.UserProfile)
                  .WithOne(up => up.User)
                  .HasForeignKey<UserProfile>(up => up.UserId);

            entity.HasMany(e => e.Medicines)
                  .WithOne(m => m.User)
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // Or Restrict if preferred

            entity.HasMany(e => e.Schedules)
                  .WithOne(s => s.User)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // Or Restrict

            entity.HasMany(e => e.Notifications)
                  .WithOne(n => n.User)
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // Or Restrict
        });

        // UserProfile Configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId); // PK is also FK
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.Gender).IsRequired();
        });

        // Medicine Configuration
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Dosage).IsRequired();

            // Relationships
            entity.HasOne(e => e.MedicineUnit)
                  .WithMany(mu => mu.Medicines)
                  .HasForeignKey(e => e.MedicineUnitId);
        });

        // MedicineUnit Configuration
        modelBuilder.Entity<MedicineUnit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired();
        });

        // Schedule Configuration
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FrequencyType).IsRequired()
                  .HasConversion<string>(); // Store enum as string

            // Relationships
            entity.HasOne(e => e.Medicine)
                  .WithMany(m => m.Schedules)
                  .HasForeignKey(e => e.MedicineId);

            entity.HasMany(e => e.ScheduleTimes)
                  .WithOne(st => st.Schedule)
                  .HasForeignKey(st => st.ScheduleId)
                  .OnDelete(DeleteBehavior.Cascade); // Cascade delete times when schedule is deleted
        });

        // ScheduleTime Configuration
        modelBuilder.Entity<ScheduleTime>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeOfDay).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();

             entity.HasMany(e => e.Notifications)
                  .WithOne(n => n.ScheduleTime)
                  .HasForeignKey(n => n.ScheduleTimeId)
                  .OnDelete(DeleteBehavior.Cascade); // Cascade delete notifications if schedule time is deleted
        });

        // Notification Configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScheduledTime).IsRequired();
            entity.Property(e => e.Message).IsRequired();
        });

        // Configure DateTime properties to use timestamp with time zone for PostgreSQL
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
                // Configure TimeOnly properties to use time without time zone
                else if (property.ClrType == typeof(TimeOnly) || property.ClrType == typeof(TimeOnly?))
                {
                     property.SetColumnType("time without time zone");
                }
            }
        }
    }
}
