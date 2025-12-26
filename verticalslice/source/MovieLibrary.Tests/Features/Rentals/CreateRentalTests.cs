using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using MovieLibrary.Data;
using MovieLibrary.Features.Rentals;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieLibrary.Tests.Features.Rentals;

public class CreateRentalTests
{
    private readonly Mock<IMovieRepository> _mockMovieRepository;
    private readonly Mock<IRentalRepository> _mockRentalRepository;
    private readonly Mock<IValidator<CreateRental.CreateRentalCommand>> _mockValidator;
    private readonly CreateRental.Handler _handler;

    public CreateRentalTests()
    {
        _mockMovieRepository = new Mock<IMovieRepository>();
        _mockRentalRepository = new Mock<IRentalRepository>();
        _mockValidator = new Mock<IValidator<CreateRental.CreateRentalCommand>>();
        _handler = new CreateRental.Handler(
            _mockMovieRepository.Object,
            _mockRentalRepository.Object,
            _mockValidator.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesRentalSuccessfully()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var command = new CreateRental.CreateRentalCommand(
            CustomerName: "John Doe",
            MovieId: movieId,
            RentalDate: DateTime.UtcNow,
            DailyRate: 3.99m
        );

        var movie = new Movie
        {
            Id = movieId,
            Title = "The Matrix",
            Director = "The Wachowskis",
            ReleaseYear = 1999,
            Genre = "Science Fiction",
            Rating = 8.7m,
            CreatedAt = DateTime.UtcNow
        };

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            CustomerName = command.CustomerName,
            MovieId = command.MovieId,
            ItemName = movie.Title,
            RentalDate = command.RentalDate,
            DailyRate = command.DailyRate,
            Status = RentalStatus.Active
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockMovieRepository
            .Setup(r => r.GetByIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        _mockRentalRepository
            .Setup(r => r.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(rental.Id);
        result.CustomerName.Should().Be(command.CustomerName);
        result.MovieId.Should().Be(command.MovieId);
        result.Message.Should().Be("Rental created successfully");

        _mockRentalRepository.Verify(
            r => r.AddAsync(It.Is<Rental>(rental =>
                rental.CustomerName == command.CustomerName &&
                rental.MovieId == command.MovieId &&
                rental.ItemName == movie.Title &&
                rental.DailyRate == command.DailyRate &&
                rental.Status == RentalStatus.Active
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_MovieNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new CreateRental.CreateRentalCommand(
            CustomerName: "John Doe",
            MovieId: Guid.NewGuid(),
            RentalDate: DateTime.UtcNow,
            DailyRate: 3.99m
        );

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockMovieRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateRental.CreateRentalCommand(
            CustomerName: "",
            MovieId: Guid.Empty,
            RentalDate: DateTime.UtcNow,
            DailyRate: -1m
        );

        var validationFailures = new[]
        {
            new ValidationFailure("CustomerName", "Customer name is required"),
            new ValidationFailure("DailyRate", "Daily rate must be greater than 0")
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Validator_MovieExists_PassesValidation()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var mockMovieRepo = new Mock<IMovieRepository>();
        mockMovieRepo
            .Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new CreateRental.Validator(mockMovieRepo.Object);
        var command = new CreateRental.CreateRentalCommand(
            CustomerName: "John Doe",
            MovieId: movieId,
            RentalDate: DateTime.UtcNow,
            DailyRate: 3.99m
        );

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_MovieDoesNotExist_FailsValidation()
    {
        // Arrange
        var movieId = Guid.NewGuid();
        var mockMovieRepo = new Mock<IMovieRepository>();
        mockMovieRepo
            .Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new CreateRental.Validator(mockMovieRepo.Object);
        var command = new CreateRental.CreateRentalCommand(
            CustomerName: "John Doe",
            MovieId: movieId,
            RentalDate: DateTime.UtcNow,
            DailyRate: 3.99m
        );

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MovieId");
    }
}
