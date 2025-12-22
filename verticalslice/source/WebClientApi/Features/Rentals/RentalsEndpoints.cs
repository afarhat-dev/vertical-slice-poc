namespace WebClientApi.Features.Rentals;

public static class RentalsEndpoints
{
    public static void MapRentalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rentals")
            .WithTags("Rentals")
            .WithOpenApi();

        group.MapPost("/", CreateRental.Handle)
            .WithName("CreateRental")
            .WithSummary("Create a new rental");

        group.MapGet("/", GetRentals.Handle)
            .WithName("GetRentals")
            .WithSummary("Get all rentals");

        group.MapGet("/{id:int}", GetRentalById.Handle)
            .WithName("GetRentalById")
            .WithSummary("Get rental by ID");

        group.MapPut("/{id:int}/return", ReturnRental.Handle)
            .WithName("ReturnRental")
            .WithSummary("Mark rental as returned");
    }
}
