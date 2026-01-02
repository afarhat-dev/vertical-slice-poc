using FluentAssertions;
using Moq;
using MovieLibrary.Features.Movies;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieLibrary.Tests.Features.Movies;

public class DeleteMovieTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly DeleteMovie.Handler _handler;

    public DeleteMovieTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _handler = new DeleteMovie.Handler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ExistingMovie_ReturnsSuccessResult()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var command = new DeleteMovie.Command(movieId);

        _mockRepository
            .Setup(r => r.DeleteAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Movie deleted successfully");

        _mockRepository.Verify(
            r => r.DeleteAsync(movieId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_NonExistentMovie_ReturnsFailureResult()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var command = new DeleteMovie.Command(movieId);

        _mockRepository
            .Setup(r => r.DeleteAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }
}
