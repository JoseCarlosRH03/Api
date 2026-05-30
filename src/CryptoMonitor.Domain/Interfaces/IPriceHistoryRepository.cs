using CryptoMonitor.Domain.Entities;

namespace CryptoMonitor.Domain.Interfaces;

public interface IPriceHistoryRepository
{
    Task AddRangeAsync(IEnumerable<PriceHistory> records, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceHistory>> GetByAssetIdAsync(
        string assetId, 
        DateTimeOffset? from, 
        DateTimeOffset? to, 
        CancellationToken cancellationToken = default
    );
    Task<decimal?> GetBasePriceAsync(string assetId, int windowHours, CancellationToken cancellationToken = default);
}
