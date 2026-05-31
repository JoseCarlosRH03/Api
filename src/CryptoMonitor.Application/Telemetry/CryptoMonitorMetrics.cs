using System.Diagnostics.Metrics;

namespace CryptoMonitor.Application.Telemetry;

public sealed class CryptoMonitorMetrics : ICryptoMonitorMetrics, IDisposable
{
    public const string MeterName = "CryptoMonitor";

    private readonly Meter _meter;
    private readonly Counter<long> _assetsSynced;
    private readonly Histogram<double> _syncDuration;
    private readonly Counter<long> _syncErrors;
    private readonly Counter<long> _alertsDetected;
    private readonly Histogram<double> _startupGap;

    public CryptoMonitorMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);

        _assetsSynced = _meter.CreateCounter<long>(
            "crypto_monitor.assets.synced",
            unit: "{assets}",
            description: "Number of assets synced from CoinCap per operation.");

        _syncDuration = _meter.CreateHistogram<double>(
            "crypto_monitor.sync.duration",
            unit: "ms",
            description: "Duration of a full CoinCap sync operation.");

        _syncErrors = _meter.CreateCounter<long>(
            "crypto_monitor.sync.errors",
            unit: "{errors}",
            description: "Number of failed sync operations.");

        _alertsDetected = _meter.CreateCounter<long>(
            "crypto_monitor.alerts.detected",
            unit: "{alerts}",
            description: "Number of price alerts detected per query.");

        _startupGap = _meter.CreateHistogram<double>(
            "crypto_monitor.startup.gap",
            unit: "min",
            description: "Minutes of price history gap detected at service startup due to downtime.");
    }

    public void RecordSync(int assetCount, double durationMs)
    {
        _assetsSynced.Add(assetCount);
        _syncDuration.Record(durationMs);
    }

    public void RecordSyncError() => _syncErrors.Add(1);

    public void RecordAlertsDetected(int alertCount) => _alertsDetected.Add(alertCount);

    public void RecordStartupGap(double gapMinutes) => _startupGap.Record(gapMinutes);

    public void Dispose() => _meter.Dispose();
}
