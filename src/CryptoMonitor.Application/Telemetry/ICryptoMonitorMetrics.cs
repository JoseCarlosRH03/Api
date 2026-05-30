namespace CryptoMonitor.Application.Telemetry;

public interface ICryptoMonitorMetrics
{
    void RecordSync(int assetCount, double durationMs);
    void RecordSyncError();
    void RecordAlertsDetected(int alertCount);
}
