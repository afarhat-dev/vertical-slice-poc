using System;

namespace MovieLibrary.Features.Rentals;

public class RentalDto(
    Guid _Id,
    string _CustomerName,
    Guid _MovieId,
    DateTime _RentalDate,
    DateTime? _ReturnDate,
    decimal _DailyRate,
    string _Status
) : BaseDto(_Id)
{
    public string CustomerName { get { return _CustomerName; } set {  _CustomerName = value; }  }
    public Guid MovieId { get { return _MovieId; } set { _MovieId = value; } }
    public DateTime RentalDate { get { return _RentalDate; } set { _RentalDate = value; } }
    public DateTime? ReturnDate { get { return _ReturnDate; } set { _ReturnDate = value; } }
    public decimal DailyRate { get { return _DailyRate; } set { _DailyRate = value; } }
    public string Status { get { return _Status; } set { _Status = value; } }

}
