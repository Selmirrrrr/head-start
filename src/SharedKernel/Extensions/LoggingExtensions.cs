using System.Diagnostics;
using Ardalis.GuardClauses;
using HeadStart.SharedKernel.Enrichers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Settings.Configuration;

namespace HeadStart.SharedKernel.Extensions;

public static class LoggingExtensions
{
    const string DefaultLoggerCfgSectionName = "Serilog";

    /// <summary>
    /// Adds Serilog with configuration that is read from appsettings.json.
    /// </summary>
    /// <param name="builder">The logger builder.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="configurationSection">The name of the configuration section.</param>
    /// <returns>An instance of <see cref="ILoggingBuilder"/>.</returns>
    public static ILoggingBuilder AddSerilog(
        this ILoggingBuilder builder, IConfiguration configuration, string configurationSection = DefaultLoggerCfgSectionName)
    {
        Guard.Against.Null(builder);

        var loggerConfiguration = new LoggerConfiguration()
            .ConfigureFromSettings(configuration, configurationSection);

#if DEBUG
        loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
#endif
        return builder.AddSerilog(loggerConfiguration.CreateLogger());
    }

    /// <summary>
    /// Appies logger configuration from appsettings.json.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="configurationSection">The name of the configuration section.</param>
    /// <returns>An instance of <see cref="LoggerConfiguration"/>.</returns>
    public static LoggerConfiguration ConfigureFromSettings(
        this LoggerConfiguration loggerConfiguration, IConfiguration configuration, string configurationSection = DefaultLoggerCfgSectionName)
    {
        Guard.Against.Null(loggerConfiguration);
        Guard.Against.Null(configuration);
        Guard.Against.NullOrWhiteSpace(configurationSection);

        var configurationReaderOptions = new ConfigurationReaderOptions
        {
            SectionName = configurationSection
        };

        return loggerConfiguration.ReadFrom.Configuration(configuration, configurationReaderOptions);
    }

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
}
