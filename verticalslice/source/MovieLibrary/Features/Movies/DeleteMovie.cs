using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Movies;

public static class DeleteMovie
{
    public record Command(Guid Id);

    public record DeleteResult(bool Success, string Message);

    public class Handler 
    {
        private readonly IMovieRepository _repository;

        public Handler(IMovieRepository repository)
        {
            _repository = repository;
        }

        public async Task<DeleteResult> ExecuteAsync(Command request, CancellationToken cancellationToken = default)
        {
            var success = await _repository.DeleteAsync(request.Id, cancellationToken);

            if (!success)
            {
                return new DeleteResult(false, $"Movie with Id {request.Id} not found");
            }

            return new DeleteResult(true, "Movie deleted successfully");
        }
    }
}
