using CryptoMonitor.API.Endpoints;
using CryptoMonitor.API.Middleware;
using CryptoMonitor.Application;
using CryptoMonitor.Application.Telemetry;
using CryptoMonitor.Infrastructure;
using CryptoMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .WriteTo.Console());

    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddOpenTelemetry()
        .WithMetrics(m => m
            .AddMeter(CryptoMonitorMetrics.MeterName)
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("System.Net.Http")
            .AddPrometheusExporter());

    builder.Services.AddOpenApi();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.IsRelational())
            db.Database.Migrate();
        else
            db.Database.EnsureCreated();
    }

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<ApiKeyMiddleware>();
    app.UseStatusCodePages();
    app.UseSerilogRequestLogging();

    app.MapPrometheusScrapingEndpoint();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    var api = app.MapGroup("/api/v1");
    api.MapGroup("/assets").MapAssetsEndpoints();
    api.MapGroup("/sync").MapSyncEndpoints();
    api.MapGroup("/alerts").MapAlertsEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
