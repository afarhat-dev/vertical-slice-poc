using System;

namespace MovieLibrary.Data;

public class Rental
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public System.DateTime RentalDate { get; set; }
    public System.DateTime? ReturnDate { get; set; }
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = "Active";
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
