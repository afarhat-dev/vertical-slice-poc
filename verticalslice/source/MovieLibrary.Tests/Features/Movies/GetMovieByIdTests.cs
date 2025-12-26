using FluentAssertions;
using Moq;
using MovieLibrary.Data;
using MovieLibrary.Features.Movies;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieLibrary.Tests.Features.Movies;

public class GetMovieByIdTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly GetMovieById.Handler _handler;

    public GetMovieByIdTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _handler = new GetMovieById.Handler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ExistingMovie_ReturnsMovieDto()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var query = new GetMovieById.Query(movieId);

        var movie = new Movie
        {
            Id = movieId,
            Title = "The Matrix",
            Director = "The Wachowskis",
            ReleaseYear = 1999,
            Genre = "Science Fiction",
            Rating = 8.7m,
            Description = "A great movie",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(movieId);
        result.Title.Should().Be("The Matrix");
        result.Director.Should().Be("The Wachowskis");
        result.ReleaseYear.Should().Be(1999);
        result.Genre.Should().Be("Science Fiction");
        result.Rating.Should().Be(8.7m);
    }

    [Fact]
    public async Task Handle_NonExistentMovie_ReturnsNull()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var query = new GetMovieById.Query(movieId);

        _mockRepository
            .Setup(r => r.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
