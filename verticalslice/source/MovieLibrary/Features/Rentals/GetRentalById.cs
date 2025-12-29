using MediatR;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Rentals;

public static class GetRentalById
{
    public record Query(Guid Id) : IRequest<RentalDto?>;

    public class Handler : IRequestHandler<Query, RentalDto?>
    {
        private readonly IRentalRepository _rentalRepository;

        public Handler(IRentalRepository rentalRepository)
        {
            _rentalRepository = rentalRepository;
        }

        public async Task<RentalDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var rental = await _rentalRepository.GetByIdAsync(request.Id, cancellationToken);

            if (rental == null)
            {
                return null;
            }

            return new RentalDto(
                rental.Id,
                rental.CustomerName,
                rental.MovieId,
                rental.RentalDate,
                rental.ReturnDate,
                rental.DailyRate,
                rental.Status,
                rental.RowVersion
            );
        }
    }
}
