namespace WebClientApi.Contracts;

public class AddMovieRequest : BaseMovieRequest
{
    public AddMovieRequest(string? title, string? director, int? releaseYear, string? genre, decimal? rating, string? description)
        : base(title, director, releaseYear, genre, rating, description)
    {
    }
}


