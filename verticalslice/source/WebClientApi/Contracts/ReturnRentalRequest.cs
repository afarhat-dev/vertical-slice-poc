namespace WebClientApi.Contracts;

public record ReturnRentalRequest(DateTime ReturnDate, byte[] RowVersion);
