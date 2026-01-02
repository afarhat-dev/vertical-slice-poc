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

public class GetAllMoviesTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly GetAllMovies.Handler _handler;

    public GetAllMoviesTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _handler = new GetAllMovies.Handler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_MoviesExist_ReturnsListOfMovieDtos()
    {
        // Arrange
        var query = new GetAllMovies.Query();

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
            },
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
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(movies);

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("The Matrix");
        result[1].Title.Should().Be("Inception");
    }

    [Fact]
    public async Task Handle_NoMovies_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllMovies.Query();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
