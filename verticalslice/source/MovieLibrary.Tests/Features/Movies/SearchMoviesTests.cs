using FluentAssertions;
using Moq;
using MovieLibrary.Data;
using MovieLibrary.Features.Movies;
using MovieLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieLibrary.Tests.Features.Movies;

public class SearchMoviesTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly SearchMovies.Handler _handler;

    public SearchMoviesTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _handler = new SearchMovies.Handler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_SearchByTitle_ReturnsMatchingMovies()
    {
        // Arrange
        var query = new SearchMovies.Query(
            Title: "Matrix",
            Director: null,
            Genre: null,
            MinYear: null,
            MaxYear: null,
            MinRating: null
        );

        var movies = new List<Movie>
        {
            new Movie
            {
                Id = Guid.NewGuid(),
                Title = "The Matrix",
                Director = "The Wachowskis",
                ReleaseYear = 1999,
                Genre = "Science Fiction",
                Rating = 8.7m,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(r => r.SearchAsync("Matrix", null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movies);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("The Matrix");
    }

    [Fact]
    public async Task Handle_SearchByMultipleCriteria_ReturnsMatchingMovies()
    {
        // Arrange
        var query = new SearchMovies.Query(
            Title: null,
            Director: "Nolan",
            Genre: "Science Fiction",
            MinYear: 2000,
            MaxYear: 2020,
            MinRating: 8.0m
        );

        var movies = new List<Movie>
        {
            new Movie
            {
                Id = Guid.NewGuid(),
                Title = "Inception",
                Director = "Christopher Nolan",
                ReleaseYear = 2010,
                Genre = "Science Fiction",
                Rating = 8.8m,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(r => r.SearchAsync(null, "Nolan", "Science Fiction", 2000, 2020, 8.0m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movies);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Inception");
        result[0].Director.Should().Be("Christopher Nolan");
    }

    [Fact]
    public async Task Handle_NoMatchingMovies_ReturnsEmptyList()
    {
        // Arrange
        var query = new SearchMovies.Query(
            Title: "NonExistent",
            Director: null,
            Genre: null,
            MinYear: null,
            MaxYear: null,
            MinRating: null
        );

        _mockRepository
            .Setup(r => r.SearchAsync("NonExistent", null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
