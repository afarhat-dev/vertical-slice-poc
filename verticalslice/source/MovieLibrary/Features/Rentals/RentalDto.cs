using System;

namespace MovieLibrary.Features.Rentals;

public record RentalDto(
    int Id,
    string CustomerName,
    string ItemName,
    DateTime RentalDate,
    DateTime? ReturnDate,
    decimal DailyRate,
    string Status
);
