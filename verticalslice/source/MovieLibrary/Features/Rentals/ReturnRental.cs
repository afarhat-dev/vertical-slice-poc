using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieLibrary.Features.Rentals;

public static class ReturnRental
{
    public record Command(int Id, DateTime ReturnDate) : IRequest<Result?>;

    public record Result(
        int Id,
        string CustomerName,
        string ItemName,
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
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid rental ID");

            RuleFor(x => x.ReturnDate)
                .NotEmpty().WithMessage("Return date is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result?>
    {
        private readonly MovieDbContext _context;
        private readonly IValidator<Command> _validator;

        public Handler(MovieDbContext context, IValidator<Command> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result?> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var rental = await _context.Rentals
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (rental is null)
                return null;

            rental.ReturnDate = request.ReturnDate;
            rental.Status = "Returned";

            await _context.SaveChangesAsync(cancellationToken);

            var days = (rental.ReturnDate.Value - rental.RentalDate).Days;
            if (days < 1) days = 1; // Minimum 1 day charge
            var totalCost = days * rental.DailyRate;

            return new Result(
                rental.Id,
                rental.CustomerName,
                rental.ItemName,
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
