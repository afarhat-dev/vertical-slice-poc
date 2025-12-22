using Microsoft.EntityFrameworkCore;
using WebClientApi.Data;

namespace WebClientApi.Features.Rentals;

public static class GetRentalById
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
        int id,
        RentalDbContext context,
        CancellationToken cancellationToken)
    {
        var rental = await context.Rentals
            .Where(r => r.Id == id)
            .Select(r => new Response(
                r.Id,
                r.CustomerName,
                r.ItemName,
                r.RentalDate,
                r.ReturnDate,
                r.DailyRate,
                r.Status
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return rental is not null
            ? Results.Ok(rental)
            : Results.NotFound();
    }
}
