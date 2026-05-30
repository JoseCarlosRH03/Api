using CryptoMonitor.Application.Alerts.Queries;
using CryptoMonitor.Application.Validators;
using FluentAssertions;

namespace CryptoMonitor.UnitTests.Validators;

public sealed class GetAlertsQueryValidatorTests
{
    private readonly GetAlertsQueryValidator _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validator_WhenWindowHoursIsNotPositive_ReturnsValidationError(int windowHours)
    {
        var result = _sut.Validate(new GetAlertsQuery(windowHours));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "WindowHours");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(24)]
    [InlineData(168)]
    public void Validator_WhenWindowHoursIsPositive_IsValid(int windowHours)
    {
        var result = _sut.Validate(new GetAlertsQuery(windowHours));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_WhenWindowHoursIsNull_IsValid()
    {
        var result = _sut.Validate(new GetAlertsQuery(null));

        result.IsValid.Should().BeTrue();
    }
}
