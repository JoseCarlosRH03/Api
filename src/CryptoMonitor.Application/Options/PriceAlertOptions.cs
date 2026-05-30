namespace CryptoMonitor.Application.Options;

public sealed class PriceAlertOptions
{
    public double ThresholdPercent { get; init; } = 5.0;
    public int WindowHours { get; init; } = 24;
    public int RetentionDays { get; init; } = 30;
}
