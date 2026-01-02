using FluentAssertions;
using Moq;
using MovieLibrary.Data;
using MovieLibrary.Features.Rentals;
using MovieLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieLibrary.Tests.Features.Rentals;

public class GetAllRentalsTests
{
    private readonly Mock<IRentalRepository> _mockRepository;
    private readonly GetAllRentals.Handler _handler;

    public GetAllRentalsTests()
    {
        _mockRepository = new Mock<IRentalRepository>();
        _handler = new GetAllRentals.Handler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_RentalsExist_ReturnsListOfRentalDtos()
    {
        // Arrange
        var query = new GetAllRentals.Query();

        var rentals = new List<Rental>
        {
            new Rental
            {
                Id = Guid.NewGuid(),
                CustomerName = "John Doe",
                MovieId = Guid.NewGuid(),
                ItemName = "The Matrix",
                RentalDate = DateTime.UtcNow.AddDays(-5),
                ReturnDate = null,
                DailyRate = 3.99m,
                Status = RentalStatus.Active
            },
            new Rental
            {
                Id = Guid.NewGuid(),
                CustomerName = "Jane Smith",
                MovieId = Guid.NewGuid(),
                ItemName = "Inception",
                RentalDate = DateTime.UtcNow.AddDays(-3),
                ReturnDate = DateTime.UtcNow,
                DailyRate = 4.99m,
                Status = RentalStatus.Returned
            }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rentals);

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].CustomerName.Should().Be("John Doe");
        result[1].CustomerName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task Handle_NoRentals_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllRentals.Query();

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Rental>());

        // Act
        var result = await _handler.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
