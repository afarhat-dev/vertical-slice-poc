using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;

namespace WebClientApi.Features.Rentals;

public static class CreateRental
{
    public record Command(
        string CustomerName,
        string ItemName,
        DateTime RentalDate,
        decimal DailyRate
    );

    public record Response(
        int Id,
        string CustomerName,
        string ItemName,
        DateTime RentalDate,
        decimal DailyRate,
        string Status
    );

    public static async Task<IResult> Handle(
        [FromBody] Command command,
        RentalDbContext context,
        CancellationToken cancellationToken)
    {
        var rental = new Rental
        {
            CustomerName = command.CustomerName,
            ItemName = command.ItemName,
            RentalDate = command.RentalDate,
            DailyRate = command.DailyRate,
            Status = "Active"
        };

        context.Rentals.Add(rental);
        await context.SaveChangesAsync(cancellationToken);

        var response = new Response(
            rental.Id,
            rental.CustomerName,
            rental.ItemName,
            rental.RentalDate,
            rental.DailyRate,
            rental.Status
        );

        return Results.Created($"/api/rentals/{rental.Id}", response);
    }
}
