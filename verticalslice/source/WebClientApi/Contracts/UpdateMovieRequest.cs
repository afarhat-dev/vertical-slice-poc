namespace MovieLibrary.Controllers;


    //TODO: Should be in contract folder
public class UpdateMovieRequest
{
    public UpdateMovieRequest(string? title, string? director, int? releaseYear, string? genre, decimal? rating, string? description, byte[] rowVersion)
    {
        this._title = title;
        this._director = director;
        this._releaseYear = releaseYear;
        this._genre = genre;
        this._rating = rating;
        this._description = description;
        this._rowVersion = rowVersion;
    }

    private string? _title;
    private string? _director;
    private int? _releaseYear;
    private string? _genre;
    private decimal? _rating;
    private string? _description;
    private byte[] _rowVersion;

    public string? Title { get => _title; set => _title = value; }
    public string? Director { get => _director; set => _director = value; }
    public int? ReleaseYear { get => _releaseYear; set => _releaseYear = value; }
    public string? Genre { get => _genre; set => _genre = value; }
    public decimal? Rating { get => _rating; set => _rating = value; }
    public string? Description { get => _description; set => _description = value; }
    public byte[] RowVersion { get => _rowVersion; set => _rowVersion = value; }
}



