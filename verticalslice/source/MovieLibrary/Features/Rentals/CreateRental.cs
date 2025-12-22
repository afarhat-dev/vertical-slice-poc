using FluentValidation;
using MediatR;
using MovieLibrary.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Rentals;

public static class CreateRental
{
    public record Command(
        string CustomerName,
        string ItemName,
        DateTime RentalDate,
        decimal DailyRate
    ) : IRequest<Result>;

    public record Result(int Id, string CustomerName, string ItemName, string Message);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer name is required")
                .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

            RuleFor(x => x.ItemName)
                .NotEmpty().WithMessage("Item name is required")
                .MaximumLength(200).WithMessage("Item name cannot exceed 200 characters");

            RuleFor(x => x.RentalDate)
                .NotEmpty().WithMessage("Rental date is required");

            RuleFor(x => x.DailyRate)
                .GreaterThan(0).WithMessage("Daily rate must be greater than 0");
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly MovieDbContext _context;
        private readonly IValidator<Command> _validator;

        public Handler(MovieDbContext context, IValidator<Command> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var rental = new Rental
            {
                CustomerName = request.CustomerName,
                ItemName = request.ItemName,
                RentalDate = request.RentalDate,
                DailyRate = request.DailyRate,
                Status = "Active"
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(rental.Id, rental.CustomerName, rental.ItemName, "Rental created successfully");
        }
    }
}
