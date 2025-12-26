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
        var existingRental = await _context.Rentals.FindAsync(new object[] { rental.Id }, cancellationToken);
        if (existingRental == null)
        {
            return false;
        }

        existingRental.ReturnDate = rental.ReturnDate;
        existingRental.Status = rental.Status;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rentals.AnyAsync(r => r.Id == id, cancellationToken);
    }
}
