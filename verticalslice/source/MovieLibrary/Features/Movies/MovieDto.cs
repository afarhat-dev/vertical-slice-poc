namespace MovieLibrary.Features.Movies;


public class MovieDto(
        int _Id,
        string _Title,
        string? _Director,
        int? _ReleaseYear,
        string? _Genre,
        decimal? _Rating,
        string? _Description,
        DateTime _CreatedAt,
        DateTime? _UpdatedAt
)
{
    public int Id { get; set; } = _Id;
    public string Title { get; set; } = _Title;
    public string? Director { get; set; } = _Director;
    public int? ReleaseYear { get; set; } = _ReleaseYear;
    public string? Genre { get; set; } = _Genre;
    public decimal? Rating { get; set; } = _Rating;
    public string? Description { get; set; } = _Description;
    public DateTime CreatedAt { get; set; } = _CreatedAt;
    public DateTime? UpdatedAt { get; set; } = _UpdatedAt;
};

