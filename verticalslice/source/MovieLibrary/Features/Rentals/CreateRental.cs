using FluentValidation;
using MovieLibrary.Data;
using MovieLibrary.Repositories;
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
    );

    public record Result(Guid Id, string CustomerName, Guid MovieId, string Message);

    public class Validator : AbstractValidator<CreateRentalCommand>
    {
        private readonly IMovieRepository _movieRepository;

        public Validator(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;

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
                    await _movieRepository.ExistsAsync(movieId, cancellationToken))
                .WithMessage("Movie with the specified ID does not exist");
        }
    }

    public class Handler 
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IRentalRepository _rentalRepository;
        private readonly IValidator<CreateRentalCommand> _validator;

        public Handler(IMovieRepository movieRepository, IRentalRepository rentalRepository, IValidator<CreateRentalCommand> validator)
        {
            _movieRepository = movieRepository;
            _rentalRepository = rentalRepository;
            _validator = validator;
        }

        public async Task<Result> ExecuteAsync(CreateRentalCommand request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Fetch the movie to get its title
            var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

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

            var addedRental = await _rentalRepository.AddAsync(rental, cancellationToken);

            return new Result(addedRental.Id, addedRental.CustomerName, addedRental.MovieId, "Rental created successfully");
        }
    }
}
