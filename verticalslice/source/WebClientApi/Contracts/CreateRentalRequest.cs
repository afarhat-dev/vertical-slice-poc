namespace WebClientApi.Contracts;

public class CreateRentalRequest
{
    public CreateRentalRequest(string customerName, Guid movieId, DateTime rentalDate, decimal dailyRate)
    {
        _customerName = customerName;
        _movieId = movieId;
        _rentalDate = rentalDate;
        _dailyRate = dailyRate;
    }

    private string _customerName;
    private Guid _movieId;
    private DateTime _rentalDate;
    private decimal _dailyRate;

    public string CustomerName { get => _customerName; set => _customerName = value; }
    public Guid MovieId { get => _movieId; set => _movieId = value; }
    public DateTime RentalDate { get => _rentalDate; set => _rentalDate = value; }
    public decimal DailyRate { get => _dailyRate; set => _dailyRate = value; }
}
