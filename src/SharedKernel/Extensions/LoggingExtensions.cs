using System.Diagnostics;
using Ardalis.GuardClauses;
using HeadStart.SharedKernel.Enrichers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Exceptions;
using Serilog.Settings.Configuration;

namespace HeadStart.SharedKernel.Extensions;

public static class LoggingExtensions
{
    private const string DefaultLoggerCfgSectionName = "Serilog";

    /// <summary>
    /// Adds the Correlatio ID enricher to the logger configuraion.
    /// </summary>
    /// <param name="enrichmentConfiguration">The logger enrichment configuration.</param>
    /// <param name="serviceProvider">The service provider used to resolve the enricher.</param>
    /// <returns>An instance of <see cref="LoggerConfiguration"/>.</returns>
    public static LoggerConfiguration WithCorrelationId(this LoggerEnrichmentConfiguration enrichmentConfiguration, IServiceProvider serviceProvider)
    {
        Guard.Against.Null(enrichmentConfiguration);

        return enrichmentConfiguration.With(serviceProvider.GetService<CorrelationIdEnricher>());
    }

    /// <summary>
    /// Adds the Instace ID enricher to the logger configuration.
    /// </summary>
    /// <param name="enrichmentConfiguration">The logger enrichment configuration.</param>
    /// <returns>An instance of <see cref="LoggerConfiguration"/>.</returns>
    public static LoggerConfiguration WithInstanceId(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        Guard.Against.Null(enrichmentConfiguration);

        return enrichmentConfiguration.With<InstanceEnricher>();
    }

    /// <summary>
    /// Configures Serilog with optimized settings for web applications.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="environment">Hosting environment.</param>
    /// <param name="applicationName">Name of the application for logging context.</param>
    /// <returns>An instance of <see cref="LoggerConfiguration"/>.</returns>
    public static LoggerConfiguration ConfigureWebApplicationLogging(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        IHostEnvironment environment,
        string applicationName)
    {
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(configuration);
        Guard.Against.Null(environment);
        Guard.Against.NullOrWhiteSpace(applicationName);

        var isDevelopment = environment.IsDevelopment();

        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("CorrelationId", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("HeadStart", isDevelopment ? Serilog.Events.LogEventLevel.Debug : Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithInstanceId()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", environment.EnvironmentName);

        if (isDevelopment)
        {
            loggerConfiguration.WriteTo.Async(a => a.Debug());
        }

        // Always add console sink wrapped in async
        loggerConfiguration.WriteTo.Async(a => a.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Application}] {Message:lj} | Correlation ID: {CorrelationId}{NewLine}{Exception}"));

        // Add OpenTelemetry sink for Aspire structured logging
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = otlpEndpoint;
                options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = applicationName
                };
            });
        }

        return loggerConfiguration;
    }
}
