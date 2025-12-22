namespace WebClientApi.Features.Rentals;

public class Rental
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = "Active";
}
