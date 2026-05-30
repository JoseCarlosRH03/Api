using System.Data;
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

    public async Task<(IReadOnlyList<PriceHistory> Items, int TotalCount)> GetPagedByAssetIdAsync(
        string assetId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.PriceHistories
            .AsNoTracking()
            .Where(p => p.AssetId == assetId);

        if (from.HasValue) query = query.Where(p => p.RecordedAt >= from.Value);

        if (to.HasValue)
        {
            query = to.Value.TimeOfDay == TimeSpan.Zero
                ? query.Where(p => p.RecordedAt < to.Value.Date.AddDays(1))
                : query.Where(p => p.RecordedAt <= to.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderBy(p => p.RecordedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (items, totalCount);
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
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var command = new CommandDefinition(sql, new { windowStart }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<(string AssetId, decimal PriceUsd)>(command).ConfigureAwait(false);

        return rows.ToDictionary(r => r.AssetId, r => r.PriceUsd);
    }

    public async Task DeleteOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        await context.PriceHistories
            .Where(p => p.RecordedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
