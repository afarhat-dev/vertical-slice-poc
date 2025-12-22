using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;

namespace WebClientApi.Features.Rentals;

public static class ReturnRental
{
    public record Command(DateTime ReturnDate);

    public record Response(
        int Id,
        string CustomerName,
        string ItemName,
        DateTime RentalDate,
        DateTime? ReturnDate,
        decimal DailyRate,
        string Status,
        decimal TotalCost
    );

    public static async Task<IResult> Handle(
        int id,
        [FromBody] Command command,
        RentalDbContext context,
        CancellationToken cancellationToken)
    {
        var rental = await context.Rentals
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rental is null)
            return Results.NotFound();

        rental.ReturnDate = command.ReturnDate;
        rental.Status = "Returned";

        await context.SaveChangesAsync(cancellationToken);

        var days = (rental.ReturnDate.Value - rental.RentalDate).Days;
        var totalCost = days * rental.DailyRate;

        var response = new Response(
            rental.Id,
            rental.CustomerName,
            rental.ItemName,
            rental.RentalDate,
            rental.ReturnDate,
            rental.DailyRate,
            rental.Status,
            totalCost
        );

        return Results.Ok(response);
    }
}
