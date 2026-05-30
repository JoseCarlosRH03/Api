namespace CryptoMonitor.Application.Options;

public sealed class CoinCapOptions
{
    public required string ApiKey { get; init; }
    public required string BaseUrl { get; init; }
    public int SyncIntervalMinutes { get; init; } = 5;
}
