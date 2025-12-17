using MediatR;
using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;

namespace WebClientApi.Features.Movies;

public static class DeleteMovie
{
    public record Command(int Id) : IRequest<Result>;

    public record Result(bool Success, string Message);

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly MovieDbContext _context;

        public Handler(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (movie == null)
            {
                return new Result(false, $"Movie with Id {request.Id} not found");
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(true, "Movie deleted successfully");
        }
    }
}
