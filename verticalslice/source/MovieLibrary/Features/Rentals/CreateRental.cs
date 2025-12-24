using FluentValidation;
using MediatR;
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
        public Validator()
        {
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer name is required")
                .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

            RuleFor(x => x.RentalDate)
                .NotEmpty().WithMessage("Rental date is required");

            RuleFor(x => x.DailyRate)
                .GreaterThan(0).WithMessage("Daily rate must be greater than 0");
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

            var rental = new Rental
            {
                CustomerName = request.CustomerName,
                MovieId = request.MovieId,
                RentalDate = request.RentalDate,
                DailyRate = request.DailyRate,
                Status = "Active"
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(rental.Id, rental.CustomerName, rental.MovieId, "Rental created successfully");
        }
    }
}
