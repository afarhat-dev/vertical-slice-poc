using System;

namespace MovieLibrary.Data;

public class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Director { get; set; }
    public int? ReleaseYear { get; set; }
    public string? Genre { get; set; }
    public decimal? Rating { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
