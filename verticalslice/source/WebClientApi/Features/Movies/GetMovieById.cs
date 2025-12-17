using MediatR;
using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;

namespace WebClientApi.Features.Movies;

public static class GetMovieById
{
    public record Query(int Id) : IRequest<MovieDto?>;

    public record MovieDto(
        int Id,
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    public class Handler : IRequestHandler<Query, MovieDto?>
    {
        private readonly MovieDbContext _context;

        public Handler(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<MovieDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var movie = await _context.Movies
                .Where(m => m.Id == request.Id)
                .Select(m => new MovieDto(
                    m.Id,
                    m.Title,
                    m.Director,
                    m.ReleaseYear,
                    m.Genre,
                    m.Rating,
                    m.Description,
                    m.CreatedAt,
                    m.UpdatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);

            return movie;
        }
    }
}
