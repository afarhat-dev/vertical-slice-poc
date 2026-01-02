using FluentAssertions;
using Moq;
using MovieLibrary.Data;
using MovieLibrary.Features.Rentals;
using MovieLibrary.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieLibrary.Tests.Features.Rentals;

public class GetRentalByIdTests
{
    private readonly Mock<IRentalRepository> _mockRepository;
    private readonly GetRentalById.Handler _handler;

    public GetRentalByIdTests()
    {
        _mockRepository = new Mock<IRentalRepository>();
        _handler = new GetRentalById.Handler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ExistingRental_ReturnsRentalDto()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var query = new GetRentalById.Query(rentalId);

        var rental = new Rental
        {
            Id = rentalId,
            CustomerName = "John Doe",
            MovieId = Guid.NewGuid(),
            ItemName = "The Matrix",
            RentalDate = DateTime.UtcNow.AddDays(-5),
            ReturnDate = null,
            DailyRate = 3.99m,
            Status = RentalStatus.Active
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rental);

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(rentalId);
        result.CustomerName.Should().Be("John Doe");
        result.DailyRate.Should().Be(3.99m);
        result.Status.Should().Be(RentalStatus.Active);
    }

    [Fact]
    public async Task Handle_NonExistentRental_ReturnsNull()
    {
        // Arrange
        var rentalId = Guid.NewGuid();
        var query = new GetRentalById.Query(rentalId);

        _mockRepository
            .Setup(r => r.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rental?)null);

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
