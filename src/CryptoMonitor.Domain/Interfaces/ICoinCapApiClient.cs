using CryptoMonitor.Domain.Entities;

namespace CryptoMonitor.Domain.Interfaces;

public interface ICoinCapApiClient
{
    Task<IReadOnlyList<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default);
}
