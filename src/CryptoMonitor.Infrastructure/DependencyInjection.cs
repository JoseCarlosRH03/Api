using CryptoMonitor.Application.Options;
using CryptoMonitor.Domain.Interfaces;
using CryptoMonitor.Infrastructure.BackgroundServices;
using CryptoMonitor.Infrastructure.CoinCap;
using CryptoMonitor.Infrastructure.Persistence;
using CryptoMonitor.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

namespace CryptoMonitor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();

        services.AddTransient<CoinCapAuthHandler>();

        services.AddHttpClient<ICoinCapApiClient, CoinCapApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<CoinCapOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + '/');
        })
        .AddHttpMessageHandler<CoinCapAuthHandler>()
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.BackoffType = DelayBackoffType.Exponential;
            options.Retry.Delay = TimeSpan.FromSeconds(1);

            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddHostedService<CoinCapSyncBackgroundService>();

        return services;
    }
}
