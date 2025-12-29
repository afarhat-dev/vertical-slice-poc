using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MovieDbContext _context;

    public MovieRepository(MovieDbContext context)
    {
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Movie>> SearchAsync(
        string? title,
        string? director,
        string? genre,
        int? minYear,
        int? maxYear,
        decimal? minRating,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Movies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(m => m.Title.Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(director))
        {
            query = query.Where(m => m.Director != null && m.Director.Contains(director));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(m => m.Genre != null && m.Genre.Contains(genre));
        }

        if (minYear.HasValue)
        {
            query = query.Where(m => m.ReleaseYear >= minYear.Value);
        }

        if (maxYear.HasValue)
        {
            query = query.Where(m => m.ReleaseYear <= maxYear.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(m => m.Rating >= minRating.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Movie> AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);
        return movie;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        // Check if the movie exists without tracking to avoid conflicts
        var exists = await _context.Movies.AsNoTracking()
            .AnyAsync(m => m.Id == movie.Id, cancellationToken);

        if (!exists)
        {
            return false;
        }

        // Set the updated timestamp
        movie.UpdatedAt = DateTime.UtcNow;

        // Attach the movie and mark it as modified
        var entry = _context.Movies.Attach(movie);
        entry.State = EntityState.Modified;

        // Set the original RowVersion value to enable EF Core's concurrency check
        entry.Property(m => m.RowVersion).OriginalValue = movie.RowVersion;

        // SaveChanges will throw DbUpdateConcurrencyException if RowVersion doesn't match
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await GetByIdAsync(id, cancellationToken);
        if (movie == null)
        {
            return false;
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies.AnyAsync(m => m.Id == id, cancellationToken);
    }
}
