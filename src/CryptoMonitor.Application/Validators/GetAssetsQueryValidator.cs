using CryptoMonitor.Application.Assets.Queries;
using FluentValidation;

namespace CryptoMonitor.Application.Validators;

internal sealed class GetAssetsQueryValidator : AbstractValidator<GetAssetsQuery>
{
    public GetAssetsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
            .WithMessage("PageSize must be between 1 and 200.");
    }
}
