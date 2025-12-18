using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Movies;

public static partial class GetAllMovies
{
    public record Query : IRequest<List<MovieDto>>;

    public class Handler : IRequestHandler<Query, List<MovieDto>>
    {
        private readonly MovieDbContext _context;

        public Handler(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovieDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var movies = await _context.Movies
                .OrderByDescending(m => m.CreatedAt)
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
                .ToListAsync(cancellationToken);

            return movies;
        }
    }
}
