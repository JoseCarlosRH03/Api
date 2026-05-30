namespace CryptoMonitor.Domain.Exceptions;

public sealed class AssetNotFoundException(string assetId)
    : Exception($"Asset '{assetId}' no se encontró.")
{
    public string AssetId { get; } = assetId;
}
