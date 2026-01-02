using Microsoft.EntityFrameworkCore;

namespace MovieLibrary.Data;

public class MovieDbContext : DbContext
{
    public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("uniqueidentifier");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Director).HasMaxLength(100);
            entity.Property(e => e.Genre).HasMaxLength(50);
            entity.Property(e => e.Rating).HasPrecision(3, 1);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.RowVersion).IsRequired().IsRowVersion();
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("uniqueidentifier");
            entity.Property(e => e.MovieId).IsRequired().HasColumnType("uniqueidentifier");
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RentalDate).IsRequired();
            entity.Property(e => e.ReturnDate);
            entity.Property(e => e.DailyRate).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RowVersion).IsRequired().IsRowVersion();

            // Configure foreign key relationship
            entity.HasOne(e => e.Movie)
                .WithMany()
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnType("uniqueidentifier");
            entity.Property(e => e.CorrelationId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.HttpMethod).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.QueryString).HasMaxLength(2000);
            entity.Property(e => e.SourceIpAddress).IsRequired().HasMaxLength(50);
            entity.Property(e => e.HostIpAddress).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StatusCode);
            entity.Property(e => e.ResponseTimeMs);

            // Create index on CorrelationId for faster lookups
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
