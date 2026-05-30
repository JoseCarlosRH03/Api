using CryptoMonitor.Application.Alerts.Queries;
using FluentValidation;

namespace CryptoMonitor.Application.Validators;

internal sealed class GetAlertsQueryValidator : AbstractValidator<GetAlertsQuery>
{
    public GetAlertsQueryValidator()
    {
        RuleFor(x => x.WindowHours)
            .GreaterThan(0)
            .When(x => x.WindowHours.HasValue)
            .WithMessage("WindowHours must be greater than 0.");
    }
}
