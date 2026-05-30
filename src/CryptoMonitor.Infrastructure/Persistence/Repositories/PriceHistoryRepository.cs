using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using Dapper;
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
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = context.PriceHistories
            .AsNoTracking()
            .Where(p => p.AssetId == assetId);

        if (from.HasValue) query = query.Where(p => p.RecordedAt >= from.Value);
        if (to.HasValue)   query = query.Where(p => p.RecordedAt <= to.Value);

        return await query.OrderBy(p => p.RecordedAt).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyDictionary<string, decimal>> GetBasePricesAsync(
        int windowHours,
        CancellationToken cancellationToken = default)
    {
        var windowStart = DateTime.UtcNow.AddHours(-windowHours);

        const string sql = """
            SELECT ph.AssetId, ph.PriceUsd
            FROM PriceHistories ph
            INNER JOIN (
                SELECT AssetId, MIN(RecordedAt) AS MinRecordedAt
                FROM PriceHistories
                WHERE RecordedAt >= @windowStart
                GROUP BY AssetId
            ) first_prices
                ON ph.AssetId = first_prices.AssetId
               AND ph.RecordedAt = first_prices.MinRecordedAt
            """;

        var connection = context.Database.GetDbConnection();
        var rows = await connection.QueryAsync<(string AssetId, decimal PriceUsd)>(
            sql, new { windowStart }).ConfigureAwait(false);

        return rows.ToDictionary(r => r.AssetId, r => r.PriceUsd);
    }
}
