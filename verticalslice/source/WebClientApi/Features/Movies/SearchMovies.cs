using MediatR;
using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;

namespace WebClientApi.Features.Movies;

public static class SearchMovies
{
    public record Query(
        string? Title,
        string? Director,
        string? Genre,
        int? MinYear,
        int? MaxYear,
        decimal? MinRating
    ) : IRequest<List<MovieDto>>;

    public record MovieDto(
        int Id,
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    );

    public class Handler : IRequestHandler<Query, List<MovieDto>>
    {
        private readonly MovieDbContext _context;

        public Handler(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovieDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                query = query.Where(m => m.Title.Contains(request.Title));
            }

            if (!string.IsNullOrWhiteSpace(request.Director))
            {
                query = query.Where(m => m.Director != null && m.Director.Contains(request.Director));
            }

            if (!string.IsNullOrWhiteSpace(request.Genre))
            {
                query = query.Where(m => m.Genre != null && m.Genre.Contains(request.Genre));
            }

            if (request.MinYear.HasValue)
            {
                query = query.Where(m => m.ReleaseYear >= request.MinYear.Value);
            }

            if (request.MaxYear.HasValue)
            {
                query = query.Where(m => m.ReleaseYear <= request.MaxYear.Value);
            }

            if (request.MinRating.HasValue)
            {
                query = query.Where(m => m.Rating >= request.MinRating.Value);
            }

            var movies = await query
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MovieDto(
                    m.Id,
                    m.Title,
                    m.Director,
                    m.ReleaseYear,
                    m.Genre,
                    m.Rating,
                    m.Description
                ))
                .ToListAsync(cancellationToken);

            return movies;
        }
    }
}
