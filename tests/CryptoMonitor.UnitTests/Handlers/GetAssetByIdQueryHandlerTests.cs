using CryptoMonitor.Application.Assets.Queries;
using CryptoMonitor.Domain.Entities;
using CryptoMonitor.Domain.Exceptions;
using CryptoMonitor.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CryptoMonitor.UnitTests.Handlers;

public sealed class GetAssetByIdQueryHandlerTests
{
    private readonly Mock<IAssetRepository> _repoMock = new();

    [Fact]
    public async Task GetAssetByIdQueryHandler_WhenAssetNotFound_ThrowsAssetNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Asset?)null);

        var handler = new GetAssetByIdQueryHandler(_repoMock.Object);
        var act = async () => await handler.Handle(new GetAssetByIdQuery("nonexistent"), CancellationToken.None);

        await act.Should().ThrowAsync<AssetNotFoundException>()
            .WithMessage("*nonexistent*");
    }
}
