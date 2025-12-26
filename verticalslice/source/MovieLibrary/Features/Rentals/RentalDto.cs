using System;

namespace MovieLibrary.Features.Rentals;

public record RentalDto(
    Guid Id,
    string CustomerName,
    Guid MovieId,
    DateTime RentalDate,
    DateTime? ReturnDate,
    decimal DailyRate,
    string Status
);
