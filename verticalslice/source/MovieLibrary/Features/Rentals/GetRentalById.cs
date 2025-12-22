using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Rentals;

public static class GetRentalById
{
    public record Query(int Id) : IRequest<RentalDto?>;

    public class Handler : IRequestHandler<Query, RentalDto?>
    {
        private readonly MovieDbContext _context;

        public Handler(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<RentalDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.Rentals
                .Where(r => r.Id == request.Id)
                .Select(r => new RentalDto(
                    r.Id,
                    r.CustomerName,
                    r.ItemName,
                    r.RentalDate,
                    r.ReturnDate,
                    r.DailyRate,
                    r.Status
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
