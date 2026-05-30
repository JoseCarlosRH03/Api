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
        // WHERE filters pushed to SQL; ORDER BY on DateTimeOffset stays client-side
        // (EF Core + SQLite cannot translate DateTimeOffset in ORDER BY clauses)
        var query = context.PriceHistories
            .AsNoTracking()
            .Where(p => p.AssetId == assetId);

        if (from.HasValue) query = query.Where(p => p.RecordedAt >= from.Value);
        if (to.HasValue) query = query.Where(p => p.RecordedAt <= to.Value);

        var records = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        return records.OrderBy(p => p.RecordedAt).ToList();
    }

    public async Task<decimal?> GetBasePriceAsync(
        string assetId,
        int windowHours,
        CancellationToken cancellationToken = default)
    {
        var windowStart = DateTimeOffset.UtcNow.AddHours(-windowHours);

        // WHERE filter pushed to SQL; ORDER BY stays client-side (SQLite DateTimeOffset limitation)
        var records = await context.PriceHistories
            .AsNoTracking()
            .Where(p => p.AssetId == assetId && p.RecordedAt >= windowStart)
            .Select(p => new { p.PriceUsd, p.RecordedAt })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records
            .OrderBy(r => r.RecordedAt)
            .Select(r => (decimal?)r.PriceUsd)
            .FirstOrDefault();
    }
}
