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

public class ReturnRentalTests
{
    private readonly Mock<IRentalRepository> _mockRepository;
    private readonly Mock<IValidator<ReturnRental.Command>> _mockValidator;
    private readonly ReturnRental.Handler _handler;

    public ReturnRentalTests()
    {
        _mockRepository = new Mock<IRentalRepository>();
        _mockValidator = new Mock<IValidator<ReturnRental.Command>>();
        _handler = new ReturnRental.Handler(_mockRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_ValidReturn_CalculatesTotalCostCorrectly()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var rentalDate = DateTime.UtcNow.AddDays(-5);
        var returnDate = DateTime.UtcNow;
        var dailyRate = 3.99m;

        var command = new ReturnRental.Command(rentalId, returnDate);

        var rental = new Rental
        {
            Id = rentalId,
            CustomerName = "John Doe",
            MovieId = Guid.NewGuid(),
            ItemName = "The Matrix",
            RentalDate = rentalDate,
            DailyRate = dailyRate,
            Status = RentalStatus.Active
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(rentalId);
        result.Status.Should().Be(RentalStatus.Returned);
        result.TotalCost.Should().BeGreaterThan(0);
        result.Message.Should().Be("Rental returned successfully");

        _mockRepository.Verify(
            r => r.UpdateAsync(It.Is<Rental>(r =>
                r.Id == rentalId &&
                r.ReturnDate == returnDate &&
                r.Status == RentalStatus.Returned
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_SameDayReturn_ChargesMinimumOneDay()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var rentalDate = DateTime.UtcNow.AddHours(-2);
        var returnDate = DateTime.UtcNow;
        var dailyRate = 3.99m;

        var command = new ReturnRental.Command(rentalId, returnDate);

        var rental = new Rental
        {
            Id = rentalId,
            CustomerName = "John Doe",
            MovieId = Guid.NewGuid(),
            ItemName = "The Matrix",
            RentalDate = rentalDate,
            DailyRate = dailyRate,
            Status = RentalStatus.Active
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TotalCost.Should().Be(dailyRate); // Minimum 1 day charge
    }

    [Fact]
    public async Task Handle_RentalNotFound_ReturnsNull()
    {
        // Arrange
        var command = new ReturnRental.Command(Guid.NewGuid(), DateTime.UtcNow);

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rental?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AlreadyReturned_ThrowsInvalidOperationException()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var command = new ReturnRental.Command(rentalId, DateTime.UtcNow);

        var rental = new Rental
        {
            Id = rentalId,
            CustomerName = "John Doe",
            MovieId = Guid.NewGuid(),
            ItemName = "The Matrix",
            RentalDate = DateTime.UtcNow.AddDays(-5),
            ReturnDate = DateTime.UtcNow.AddDays(-1),
            DailyRate = 3.99m,
            Status = RentalStatus.Returned
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnDateBeforeRentalDate_ThrowsValidationException()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var rentalDate = DateTime.UtcNow;
        var returnDate = DateTime.UtcNow.AddDays(-1);

        var command = new ReturnRental.Command(rentalId, returnDate);

        var rental = new Rental
        {
            Id = rentalId,
            CustomerName = "John Doe",
            MovieId = Guid.NewGuid(),
            ItemName = "The Matrix",
            RentalDate = rentalDate,
            DailyRate = 3.99m,
            Status = RentalStatus.Active
        };

        _mockValidator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository
            .Setup(r => r.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
