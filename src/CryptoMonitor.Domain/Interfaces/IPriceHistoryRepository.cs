using CryptoMonitor.Domain.Entities;

namespace CryptoMonitor.Domain.Interfaces;

public interface IPriceHistoryRepository
{
    Task AddRangeAsync(IEnumerable<PriceHistory> records, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<PriceHistory> Items, int TotalCount)> GetPagedByAssetIdAsync(
        string assetId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, decimal>> GetBasePricesAsync(int windowHours, CancellationToken cancellationToken = default);
    Task DeleteOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default);
}
