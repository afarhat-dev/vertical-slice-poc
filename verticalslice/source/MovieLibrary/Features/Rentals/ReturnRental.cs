using FluentValidation;
using MovieLibrary.Data;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Rentals;

public static class ReturnRental
{
    public record Command(Guid Id, DateTime ReturnDate, byte[] RowVersion);

    public record ReturnRentalResult(
        Guid Id,
        string CustomerName,
        Guid MovieId,
        DateTime RentalDate,
        DateTime ReturnDate,
        decimal DailyRate,
        string Status,
        decimal TotalCost,
        string Message
    );

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ReturnDate)
                .NotEmpty().WithMessage("Return date is required")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Return date cannot be more than 1 day in the future");
        }
    }

    public class Handler 
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IValidator<Command> _validator;

        public Handler(IRentalRepository rentalRepository, IValidator<Command> validator)
        {
            _rentalRepository = rentalRepository;
            _validator = validator;
        }

        public async Task<ReturnRentalResult?> ExecuteAsync(Command request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var rental = await _rentalRepository.GetByIdAsNoTrackingAsync(request.Id, cancellationToken);

            if (rental is null)
                return null;

            // Check if rental is already returned
            if (rental.Status == RentalStatus.Returned)
            {
                throw new InvalidOperationException("This rental has already been returned");
            }

            // Validate return date is not before rental date
            if (request.ReturnDate < rental.RentalDate)
            {
                throw new ValidationException("Return date cannot be before rental date");
            }

            // Create updated rental with RowVersion for concurrency check
            rental.ReturnDate = request.ReturnDate;
            rental.Status = RentalStatus.Returned;
            rental.RowVersion = request.RowVersion;

            await _rentalRepository.UpdateAsync(rental, cancellationToken);

            var days = (rental.ReturnDate.Value - rental.RentalDate).Days;
            if (days < 1) days = 1; // Minimum 1 day charge
            var totalCost = days * rental.DailyRate;

            return new ReturnRentalResult(
                rental.Id,
                rental.CustomerName,
                rental.MovieId,
                rental.RentalDate,
                rental.ReturnDate.Value,
                rental.DailyRate,
                rental.Status,
                totalCost,
                "Rental returned successfully"
            );
        }
    }
}
