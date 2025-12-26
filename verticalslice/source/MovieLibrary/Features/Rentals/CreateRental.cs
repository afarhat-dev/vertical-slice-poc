using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Rentals;

public static class CreateRental
{
    public record CreateRentalCommand(
        string CustomerName,
        Guid MovieId,
        DateTime RentalDate,
        decimal DailyRate
    ) : IRequest<Result>;

    public record Result(Guid Id, string CustomerName, Guid MovieId, string Message);

    public class Validator : AbstractValidator<CreateRentalCommand>
    {
        private readonly MovieDbContext _context;

        public Validator(MovieDbContext context)
        {
            _context = context;

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer name is required")
                .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

            RuleFor(x => x.RentalDate)
                .NotEmpty().WithMessage("Rental date is required")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Rental date cannot be more than 1 day in the future")
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-1)).WithMessage("Rental date cannot be more than 1 year in the past");

            RuleFor(x => x.DailyRate)
                .GreaterThan(0).WithMessage("Daily rate must be greater than 0");

            RuleFor(x => x.MovieId)
                .MustAsync(async (movieId, cancellationToken) =>
                    await _context.Movies.AnyAsync(m => m.Id == movieId, cancellationToken))
                .WithMessage("Movie with the specified ID does not exist");
        }
    }

    public class Handler : IRequestHandler<CreateRentalCommand, Result>
    {
        private readonly MovieDbContext _context;
        private readonly IValidator<CreateRentalCommand> _validator;

        public Handler(MovieDbContext context, IValidator<CreateRentalCommand> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(CreateRentalCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Fetch the movie to get its title
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken);

            if (movie == null)
            {
                throw new InvalidOperationException("Movie not found");
            }

            var rental = new Rental
            {
                CustomerName = request.CustomerName,
                MovieId = request.MovieId,
                ItemName = movie.Title,
                RentalDate = request.RentalDate,
                DailyRate = request.DailyRate,
                Status = RentalStatus.Active
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(rental.Id, rental.CustomerName, rental.MovieId, "Rental created successfully");
        }
    }
}
