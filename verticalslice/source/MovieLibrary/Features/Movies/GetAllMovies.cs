using MediatR;
using MovieLibrary.Repositories;
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
        private readonly IMovieRepository _repository;

        public Handler(IMovieRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MovieDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var movies = await _repository.GetAllAsync(cancellationToken);

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
