using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoMonitor.Infrastructure.Persistence.Repositories;

internal sealed class PriceHistoryRepository(AppDbContext context) : IPriceHistoryRepository
{
    public async Task AddRangeAsync(IEnumerable<PriceHistory> records, CancellationToken cancellationToken = default)
    {
        context.PriceHistories.AddRange(records);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PriceHistory>> GetByAssetIdAsync(
        string assetId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default
    )
    {
        var records = await context.PriceHistories
            .AsNoTracking()
            .Where(p => p.AssetId == assetId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // DateTimeOffset ordering done client-side (SQLite EF Core limitation)
        var query = records.AsEnumerable();
        if (from.HasValue) query = query.Where(p => p.RecordedAt >= from.Value);
        if (to.HasValue) query = query.Where(p => p.RecordedAt <= to.Value);
        return query.OrderBy(p => p.RecordedAt).ToList();
    }

    public async Task<decimal?> GetBasePriceAsync(
        string assetId,
        int windowHours,
        CancellationToken cancellationToken = default)
    {
        var windowStart = DateTimeOffset.UtcNow.AddHours(-windowHours);

        var records = await context.PriceHistories
            .AsNoTracking()
            .Where(p => p.AssetId == assetId)
            .Select(p => new { p.PriceUsd, p.RecordedAt })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records
            .Where(r => r.RecordedAt >= windowStart)
            .OrderBy(r => r.RecordedAt)
            .Select(r => (decimal?)r.PriceUsd)
            .FirstOrDefault();
    }
}
