using MediatR;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Movies;

public static class GetMovieById
{
    public record Query(Guid Id) : IRequest<MovieDto?>;


    public class Handler : IRequestHandler<Query, MovieDto?>
    {
        private readonly IMovieRepository _repository;

        public Handler(IMovieRepository repository)
        {
            _repository = repository;
        }

        public async Task<MovieDto?> Handle(Query request, CancellationToken cancellationToken)
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
