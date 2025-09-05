using Microsoft.EntityFrameworkCore;
using Mtu.Rentals.Domain.Entities;

namespace Mtu.Rentals.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<Motorcycle> Motorcycles => Set<Motorcycle>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<MotorcycleNotification> MotorcycleNotifications => Set<MotorcycleNotification>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.HasPostgresExtension("uuid-ossp");
        b.UseSerialColumns();

        // Motorcycle
        b.Entity<Motorcycle>(e =>
        {
            e.HasKey(x => x.Id);

            e.OwnsOne(x => x.Plate, p =>
            {
                p.Property(pp => pp.Value)
                    .HasColumnName("plate")
                    .HasMaxLength(16)
                    .IsRequired();

                p.Property(pp => pp.Normalized)
                    .HasColumnName("plate_normalized")
                    .HasMaxLength(16)
                    .IsRequired();
                
                // ⬇️ O índice deve ser definido no owned type
                p.HasIndex(pp => pp.Normalized)
                .IsUnique()
                .HasDatabaseName("ix_motorcycles_plate_normalized");
            });
        });

        // Courier
        b.Entity<Courier>(e =>
        {
            e.HasKey(x => x.Id);

            e.OwnsOne(x => x.Cnpj, p =>
            {
                p.Property(pp => pp.Digits)
                    .HasColumnName("cnpj")
                    .HasMaxLength(14)
                    .IsRequired();

                // ⬇️ Índice do owned type
                p.HasIndex(pp => pp.Digits)
                .IsUnique()
                .HasDatabaseName("ix_couriers_cnpj");
            });

            e.Property(x => x.CnhNumber)
                .IsRequired();

            e.HasIndex(x => x.CnhNumber)
            .IsUnique();
        });

        // Rental
        b.Entity<Rental>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Plan).HasConversion<int>();
        });

        // MotorcycleNotification
        b.Entity<MotorcycleNotification>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.MotorcycleId).IsUnique();
        });
    }
}

public sealed class MotorcycleNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MotorcycleId { get; set; }
    public int Year { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}