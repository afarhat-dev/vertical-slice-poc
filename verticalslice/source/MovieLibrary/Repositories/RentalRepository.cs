using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly MovieDbContext _context;

    public RentalRepository(MovieDbContext context)
    {
        _context = context;
    }

    public async Task<Rental?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rentals
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Rental?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rentals
            .AsNoTracking()
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Rental>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Rentals
            .Include(r => r.Movie)
            .ToListAsync(cancellationToken);
    }

    public async Task<Rental> AddAsync(Rental rental, CancellationToken cancellationToken = default)
    {
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync(cancellationToken);
        return rental;
    }

    public async Task<bool> UpdateAsync(Rental rental, CancellationToken cancellationToken = default)
    {
        // Check if the rental exists without tracking to avoid conflicts
        var exists = await _context.Rentals.AsNoTracking()
            .AnyAsync(r => r.Id == rental.Id, cancellationToken);

        if (!exists)
        {
            return false;
        }

        // Attach the rental and mark it as modified
        var entry = _context.Rentals.Attach(rental);
        entry.State = EntityState.Modified;

        // Set the original RowVersion value to enable EF Core's concurrency check
        entry.Property(r => r.RowVersion).OriginalValue = rental.RowVersion;

        // SaveChanges will throw DbUpdateConcurrencyException if RowVersion doesn't match
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rentals.AnyAsync(r => r.Id == id, cancellationToken);
    }
}
