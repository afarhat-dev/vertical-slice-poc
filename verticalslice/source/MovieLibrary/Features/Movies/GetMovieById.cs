using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Movies;

public static class GetMovieById
{
    public record Query(Guid Id);


    public class Handler 
    {
        private readonly IMovieRepository _repository;

        public Handler(IMovieRepository repository)
        {
            _repository = repository;
        }

        public async Task<MovieDto?> ExecuteAsync(Query request, CancellationToken cancellationToken = default)
        {
            var movie = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (movie == null)
            {
                return null;
            }

            return new MovieDto(
                movie.Id,
                movie.Title,
                movie.Director,
                movie.ReleaseYear,
                movie.Genre,
                movie.Rating,
                movie.Description,
                movie.CreatedAt,
                movie.UpdatedAt
            ) { RowVersion = movie.RowVersion };
        }
    }
}
