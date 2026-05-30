using CryptoMonitor.Application.Behaviors;
using CryptoMonitor.Application.Interfaces;
using CryptoMonitor.Application.Options;
using CryptoMonitor.Application.Services;
using CryptoMonitor.Application.Telemetry;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoMonitor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddOptions<PriceAlertOptions>()
            .Bind(configuration.GetSection("PriceAlert"))
            .Validate(options => options.ThresholdPercent > 0, "PriceAlert:ThresholdPercent must be greater than 0.")
            .Validate(options => options.WindowHours > 0, "PriceAlert:WindowHours must be greater than 0.")
            .Validate(options => options.RetentionDays >= 0, "PriceAlert:RetentionDays cannot be negative.")
            .ValidateOnStart();

        services.AddOptions<CoinCapOptions>()
            .Bind(configuration.GetSection("CoinCap"))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "CoinCap:ApiKey is required.")
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "CoinCap:BaseUrl must be an absolute URL.")
            .Validate(options => options.SyncIntervalMinutes > 0, "CoinCap:SyncIntervalMinutes must be greater than 0.")
            .ValidateOnStart();

        services.AddOptions<ApiSecurityOptions>()
            .Bind(configuration.GetSection("ApiSecurity"))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "ApiSecurity:ApiKey is required.")
            .ValidateOnStart();

        services.AddScoped<IAlertDetectionService, AlertDetectionService>();

        services.AddMetrics();
        services.AddSingleton<ICryptoMonitorMetrics, CryptoMonitorMetrics>();

        return services;
    }
}
