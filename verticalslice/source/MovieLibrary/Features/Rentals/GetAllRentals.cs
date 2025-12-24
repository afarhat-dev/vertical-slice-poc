using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Rentals;

public static class GetAllRentals
{
    public record Query() : IRequest<List<RentalDto>>;

    public class Handler : IRequestHandler<Query, List<RentalDto>>
    {
        private readonly MovieDbContext _context;

        public Handler(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<List<RentalDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.Rentals
                .Select(r => new RentalDto(
                    r.Id,
                    r.CustomerName,
                    r.MovieId,
                    r.RentalDate,
                    r.ReturnDate,
                    r.DailyRate,
                    r.Status
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
