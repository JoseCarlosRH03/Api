using CryptoMonitor.Application.Assets.Queries;
using FluentValidation;

namespace CryptoMonitor.Application.Validators;

internal sealed class GetAssetHistoryQueryValidator : AbstractValidator<GetAssetHistoryQuery>
{
    public GetAssetHistoryQueryValidator()
    {
        RuleFor(x => x.AssetId)
            .NotEmpty()
            .WithMessage("AssetId is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 500)
            .WithMessage("PageSize must be between 1 and 500.");

        When(x => x.From.HasValue && x.To.HasValue, () =>
            RuleFor(x => x.From!.Value)
                .LessThan(x => x.To!.Value)
                .WithMessage("'From' must be earlier than 'To'."));
    }
}
