using MediatR;
using MovieLibrary.Repositories;
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
        private readonly IRentalRepository _rentalRepository;

        public Handler(IRentalRepository rentalRepository)
        {
            _rentalRepository = rentalRepository;
        }

        public async Task<List<RentalDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var rentals = await _rentalRepository.GetAllAsync(cancellationToken);

            return rentals.Select(r => new RentalDto(
                r.Id,
                r.CustomerName,
                r.MovieId,
                r.RentalDate,
                r.ReturnDate,
                r.DailyRate,
                r.Status
            )).ToList();
        }
    }
}
