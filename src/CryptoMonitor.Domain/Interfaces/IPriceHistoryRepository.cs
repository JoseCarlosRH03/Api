using CryptoMonitor.Domain.Entities;

namespace CryptoMonitor.Domain.Interfaces;

public interface IPriceHistoryRepository
{
    Task AddRangeAsync(IEnumerable<PriceHistory> records, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriceHistory>> GetByAssetIdAsync(
        string assetId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyDictionary<string, decimal>> GetBasePricesAsync(int windowHours, CancellationToken cancellationToken = default);
}
