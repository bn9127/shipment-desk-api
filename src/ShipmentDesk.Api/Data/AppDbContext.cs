using Microsoft.EntityFrameworkCore;
using ShipmentDesk.Api.Domain;

namespace ShipmentDesk.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Shipment> Shipments => Set<Shipment>();

    public DbSet<Milestone> Milestones => Set<Milestone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(shipment => shipment.Id);
            entity.HasIndex(shipment => shipment.Reference).IsUnique();
            entity.Property(shipment => shipment.Reference).HasMaxLength(20).IsRequired();
            entity.Property(shipment => shipment.Origin).HasMaxLength(3).IsRequired();
            entity.Property(shipment => shipment.Destination).HasMaxLength(3).IsRequired();
            entity.Property(shipment => shipment.Shipper).HasMaxLength(120).IsRequired();
            entity.Property(shipment => shipment.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasMany(shipment => shipment.Milestones)
                .WithOne(milestone => milestone.Shipment!)
                .HasForeignKey(milestone => milestone.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.HasKey(milestone => milestone.Id);
            entity.Property(milestone => milestone.Location).HasMaxLength(80).IsRequired();
            entity.Property(milestone => milestone.Note).HasMaxLength(200);
            entity.Property(milestone => milestone.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
