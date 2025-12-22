using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;

namespace WebClientApi.Features.Rentals;

public static class GetRentals
{
    public record Response(
        int Id,
        string CustomerName,
        string ItemName,
        DateTime RentalDate,
        DateTime? ReturnDate,
        decimal DailyRate,
        string Status
    );

    public static async Task<IResult> Handle(
        RentalDbContext context,
        CancellationToken cancellationToken)
    {
        var rentals = await context.Rentals
            .Select(r => new Response(
                r.Id,
                r.CustomerName,
                r.ItemName,
                r.RentalDate,
                r.ReturnDate,
                r.DailyRate,
                r.Status
            ))
            .ToListAsync(cancellationToken);

        return Results.Ok(rentals);
    }
}
