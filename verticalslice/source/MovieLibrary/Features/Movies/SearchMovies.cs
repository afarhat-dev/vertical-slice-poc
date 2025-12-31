using MovieLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Movies;

public static class SearchMovies
{
    public record Query(
        string? Title,
        string? Director,
        string? Genre,
        int? MinYear,
        int? MaxYear,
        decimal? MinRating
    )>;
 

    public class Handler >
    {
        private readonly IMovieRepository _repository;

        public Handler(IMovieRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MovieDto>> Handle(Query request, CancellationToken cancellationToken = default)
        {
            var movies = await _repository.SearchAsync(
                request.Title,
                request.Director,
                request.Genre,
                request.MinYear,
                request.MaxYear,
                request.MinRating,
                cancellationToken
            );

            return movies.Select(m => new MovieDto(
                m.Id,
                m.Title,
                m.Director,
                m.ReleaseYear,
                m.Genre,
                m.Rating,
                m.Description,
                m.CreatedAt,
                m.UpdatedAt
            ) { RowVersion = m.RowVersion }).ToList();
        }
    }
}
