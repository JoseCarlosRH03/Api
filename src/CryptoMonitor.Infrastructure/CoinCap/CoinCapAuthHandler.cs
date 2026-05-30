using System.Net.Http.Headers;
using CryptoMonitor.Application.Options;
using Microsoft.Extensions.Options;

namespace CryptoMonitor.Infrastructure.CoinCap;

internal sealed class CoinCapAuthHandler(IOptions<CoinCapOptions> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);

        return base.SendAsync(request, cancellationToken);
    }
}
