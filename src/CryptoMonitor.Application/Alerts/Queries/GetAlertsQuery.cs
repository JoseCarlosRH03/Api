using CryptoMonitor.Application.DTOs;
using CryptoMonitor.Application.Interfaces;
using MediatR;

namespace CryptoMonitor.Application.Alerts.Queries;

public sealed record GetAlertsQuery(int? WindowHours = null) : IRequest<IReadOnlyList<AlertDto>>;

internal sealed class GetAlertsQueryHandler(IAlertDetectionService alertDetectionService)
    : IRequestHandler<GetAlertsQuery, IReadOnlyList<AlertDto>>
{
    public async Task<IReadOnlyList<AlertDto>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        return await alertDetectionService.DetectAlertsAsync(request.WindowHours, cancellationToken);
    }
}
