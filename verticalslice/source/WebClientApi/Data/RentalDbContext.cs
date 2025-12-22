using Microsoft.EntityFrameworkCore;
using WebClientApi.Features.Rentals;

namespace WebClientApi.Data;

public class RentalDbContext : DbContext
{
    public RentalDbContext(DbContextOptions<RentalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Rental> Rentals => Set<Rental>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.RentalDate)
                .IsRequired();
            entity.Property(e => e.ReturnDate);
            entity.Property(e => e.DailyRate)
                .HasPrecision(18, 2);
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);
        });
    }
}
