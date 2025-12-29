namespace WebClientApi.Contracts;

public class UpdateMovieRequest : BaseMovieRequest
{
    public UpdateMovieRequest(string? title, string? director, int? releaseYear, string? genre, decimal? rating, string? description, byte[] rowVersion)
        : base(title, director, releaseYear, genre, rating, description)
    {
        _rowVersion = rowVersion;
    }

    private byte[] _rowVersion;

    public byte[] RowVersion { get => _rowVersion; set => _rowVersion = value; }
}



