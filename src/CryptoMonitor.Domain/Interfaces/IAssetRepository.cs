using CryptoMonitor.Domain.Entities;

namespace CryptoMonitor.Domain.Interfaces;

public interface IAssetRepository
{
    Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Asset> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Asset?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task UpsertRangeAsync(IEnumerable<Asset> assets, CancellationToken cancellationToken = default);
}
