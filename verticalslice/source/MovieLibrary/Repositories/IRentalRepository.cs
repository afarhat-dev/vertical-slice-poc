using MovieLibrary.Data;

namespace MovieLibrary.Repositories;

public interface IRentalRepository
{
    Task<Rental?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Rental>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Rental> AddAsync(Rental rental, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Rental rental, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
