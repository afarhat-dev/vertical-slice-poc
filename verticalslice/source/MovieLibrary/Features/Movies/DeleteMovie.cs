using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Movies;

public static class DeleteMovie
{
    public record Command(Guid Id) : IRequest<DeleteResult>;

    public record DeleteResult(bool Success, string Message);

    public class Handler : IRequestHandler<Command, DeleteResult>
    {
        private readonly MovieDbContext _context;

        public Handler(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<DeleteResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (movie == null)
            {
                return new DeleteResult(false, $"Movie with Id {request.Id} not found");
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeleteResult(true, "Movie deleted successfully");
        }
    }
}
