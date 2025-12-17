namespace WebClientApi.Controllers;

public partial class MoviesController
{
    //TODO: Should be in contract folder
    public record UpdateMovieRequest(
        string Title,
        string? Director,
        int? ReleaseYear,
        string? Genre,
        decimal? Rating,
        string? Description
    );
}
