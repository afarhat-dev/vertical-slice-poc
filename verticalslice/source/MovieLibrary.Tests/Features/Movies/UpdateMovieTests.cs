using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieLibrary.Data;
using MovieLibrary.Features.Movies;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieLibrary.Tests.Features.Movies;

public class UpdateMovieTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly Mock<IValidator<UpdateMovie.UpdateCommand>> _mockValidator;
    private readonly UpdateMovie.Handler _handler;

    public UpdateMovieTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _mockValidator = new Mock<IValidator<UpdateMovie.UpdateCommand>>();
        _handler = new UpdateMovie.Handler(_mockRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var command = new UpdateMovie.UpdateCommand(
            Id: movieId,
            Title: "The Matrix Reloaded",
            Director: "The Wachowskis",
            ReleaseYear: 2003,
            Genre: "Science Fiction",
            Rating: 7.2m,
            Description: "Updated description",
            RowVersion: new byte[] { 1, 2, 3, 4 }
        );

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Movie updated successfully");

        _mockRepository.Verify(
            r => r.UpdateAsync(It.Is<Movie>(m =>
                m.Id == movieId &&
                m.Title == command.Title &&
                m.Director == command.Director
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_MovieNotFound_ReturnsFailureResult()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var command = new UpdateMovie.UpdateCommand(
            Id: movieId,
            Title: "Non-existent Movie",
            Director: "Nobody",
            ReleaseYear: 2020,
            Genre: "Unknown",
            Rating: 5m,
            Description: "Does not exist",
            RowVersion: new byte[] { 1, 2, 3, 4 }
        );

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateMovie.UpdateCommand(
            Id: Guid.NewGuid(),
            Title: "",
            Director: null,
            ReleaseYear: null,
            Genre: null,
            Rating: null,
            Description: null,
            RowVersion: new byte[] { 1, 2, 3, 4 }
        );

        var validationFailures = new[]
        {
            new ValidationFailure("Title", "Title is required")
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.ExecuteAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ConcurrentUpdate_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var command = new UpdateMovie.UpdateCommand(
            Id: movieId,
            Title: "The Matrix Reloaded",
            Director: "The Wachowskis",
            ReleaseYear: 2003,
            Genre: "Science Fiction",
            Rating: 7.2m,
            Description: "Updated description",
            RowVersion: new byte[] { 1, 2, 3, 4 }
        );

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            _handler.ExecuteAsync(command, CancellationToken.None));
    }
}
