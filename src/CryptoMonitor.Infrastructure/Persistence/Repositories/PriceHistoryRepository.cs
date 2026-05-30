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
        var query = context.PriceHistories
            .AsNoTracking()
            .Where(p => p.AssetId == assetId);

        if (from.HasValue)
            query = query.Where(p => p.RecordedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.RecordedAt <= to.Value);

        return await query
            .OrderBy(p => p.RecordedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<decimal?> GetBasePriceAsync(
        string assetId,
        int windowHours,
        CancellationToken cancellationToken = default)
    {
        var windowStart = DateTimeOffset.UtcNow.AddHours(-windowHours);

        return await context.PriceHistories
            .AsNoTracking()
            .Where(p => p.AssetId == assetId && p.RecordedAt >= windowStart)
            .OrderBy(p => p.RecordedAt)
            .Select(p => (decimal?)p.PriceUsd)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
