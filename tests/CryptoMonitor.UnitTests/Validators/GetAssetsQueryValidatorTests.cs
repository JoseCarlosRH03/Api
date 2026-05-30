using CryptoMonitor.Application.Assets.Queries;
using CryptoMonitor.Application.Validators;
using FluentAssertions;

namespace CryptoMonitor.UnitTests.Validators;

public sealed class GetAssetsQueryValidatorTests
{
    private readonly GetAssetsQueryValidator _sut = new();

    [Theory]
    [InlineData(0, 50)]
    [InlineData(-1, 50)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 201)]
    public void Validator_WithInvalidPagination_ReturnsValidationError(int page, int pageSize)
    {
        var result = _sut.Validate(new GetAssetsQuery(page, pageSize));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 50)]
    [InlineData(5, 200)]
    [InlineData(100, 100)]
    public void Validator_WithValidPagination_IsValid(int page, int pageSize)
    {
        var result = _sut.Validate(new GetAssetsQuery(page, pageSize));

        result.IsValid.Should().BeTrue();
    }
}
