using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CryptoMonitor.Infrastructure.Persistence.Repositories;

internal sealed class AssetRepository(AppDbContext context) : IAssetRepository
{
    public async Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Assets
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Asset?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpsertRangeAsync(IEnumerable<Asset> assets, CancellationToken cancellationToken = default)
    {
        var assetList = assets.ToList();

        var existingIds = await context.Assets
            .AsNoTracking()
            .Select(a => a.Id)
            .ToHashSetAsync(cancellationToken)
            .ConfigureAwait(false);

        var toAdd = assetList.Where(a => !existingIds.Contains(a.Id)).ToList();
        var toUpdate = assetList.Where(a => existingIds.Contains(a.Id)).ToList();

        if (toAdd.Count > 0)
            context.Assets.AddRange(toAdd);

        if (toUpdate.Count > 0)
            context.Assets.UpdateRange(toUpdate);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
