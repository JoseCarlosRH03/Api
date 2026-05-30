using CryptoMonitor.Application.DTOs;

namespace CryptoMonitor.Application.Interfaces;

public interface IAlertDetectionService
{
    Task<IReadOnlyList<AlertDto>> DetectAlertsAsync(int? windowHours = null, CancellationToken cancellationToken = default);
}
