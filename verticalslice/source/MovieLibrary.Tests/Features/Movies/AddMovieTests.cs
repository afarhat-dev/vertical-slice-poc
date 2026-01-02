using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using MovieLibrary.Data;
using MovieLibrary.Features.Movies;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieLibrary.Tests.Features.Movies;

public class AddMovieTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly Mock<IValidator<AddMovie.AddCommand>> _mockValidator;
    private readonly AddMovie.Handler _handler;

    public AddMovieTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _mockValidator = new Mock<IValidator<AddMovie.AddCommand>>();
        _handler = new AddMovie.Handler(_mockRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var command = new AddMovie.AddCommand(
            Title: "The Matrix",
            Director: "The Wachowskis",
            ReleaseYear: 1999,
            Genre: "Science Fiction",
            Rating: 8.7m,
            Description: "A computer hacker learns about the true nature of reality"
        );

        var expectedMovie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Director = command.Director,
            ReleaseYear = command.ReleaseYear,
            Genre = command.Genre,
            Rating = command.Rating,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMovie);

        // Act
        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedMovie.Id);
        result.Title.Should().Be(command.Title);
        result.Message.Should().Be("Movie added successfully");

        _mockRepository.Verify(
            r => r.AddAsync(It.Is<Movie>(m =>
                m.Title == command.Title &&
                m.Director == command.Director &&
                m.ReleaseYear == command.ReleaseYear &&
                m.Genre == command.Genre &&
                m.Rating == command.Rating &&
                m.Description == command.Description
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var command = new AddMovie.AddCommand(
            Title: "",
            Director: null,
            ReleaseYear: null,
            Genre: null,
            Rating: null,
            Description: null
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

        _mockRepository.Verify(
            r => r.AddAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public void Validator_EmptyTitle_ReturnsValidationError()
    {
        // Arrange
        var validator = new AddMovie.Validator();
        var command = new AddMovie.AddCommand(
            Title: "",
            Director: null,
            ReleaseYear: null,
            Genre: null,
            Rating: null,
            Description: null
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validator_TitleExceedsMaxLength_ReturnsValidationError()
    {
        // Arrange
        var validator = new AddMovie.Validator();
        var command = new AddMovie.AddCommand(
            Title: new string('A', 201),
            Director: null,
            ReleaseYear: null,
            Genre: null,
            Rating: null,
            Description: null
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Title" &&
            e.ErrorMessage.Contains("200 characters"));
    }

    [Fact]
    public void Validator_RatingOutOfRange_ReturnsValidationError()
    {
        // Arrange
        var validator = new AddMovie.Validator();
        var command = new AddMovie.AddCommand(
            Title: "Test Movie",
            Director: null,
            ReleaseYear: null,
            Genre: null,
            Rating: 11m,
            Description: null
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rating");
    }

    [Fact]
    public void Validator_ValidCommand_PassesValidation()
    {
        // Arrange
        var validator = new AddMovie.Validator();
        var command = new AddMovie.AddCommand(
            Title: "The Matrix",
            Director: "The Wachowskis",
            ReleaseYear: 1999,
            Genre: "Science Fiction",
            Rating: 8.7m,
            Description: "A great movie"
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
