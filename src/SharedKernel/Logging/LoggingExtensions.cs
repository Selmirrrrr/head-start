using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace HeadStart.SharedKernel.Logging;

public static class LoggingExtensions
{
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

        IDictionary<string, ColumnWriterBase> columnOptions = new Dictionary<string, ColumnWriterBase>
        {
            { "Id", new GuidColumnWriter() },
            { "TraceId", new SinglePropertyColumnWriter("RequestId", PropertyWriteMethod.Raw, NpgsqlDbType.Varchar) },
            { "DateUtc", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            { "RequestPath", new SinglePropertyColumnWriter("Path", PropertyWriteMethod.Raw, NpgsqlDbType.Varchar) },
            { "RequestMethod", new SinglePropertyColumnWriter("Method", PropertyWriteMethod.Raw, NpgsqlDbType.Varchar) },
            { "ResponseStatusCode", new SinglePropertyColumnWriter("StatusCode", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "RequestQuery", new SinglePropertyColumnWriter("QueryString", PropertyWriteMethod.Raw, NpgsqlDbType.Varchar) },
            { "RequestBody", new SinglePropertyColumnWriter("RequestBody", PropertyWriteMethod.Raw, NpgsqlDbType.Jsonb) },
            { "UserId", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.Raw, NpgsqlDbType.Uuid) },
            { "ImpersonatedByUserId", new SinglePropertyColumnWriter("ImpersonatedByUserId", PropertyWriteMethod.Raw, NpgsqlDbType.Uuid) },
            { "TenantPath", new TenantPathColumnWriter() },
        };

        var isDevelopment = environment.IsDevelopment();

        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogEventLevel.Information)
            .MinimumLevel.Override("HeadStart", isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information)
            .Filter.ByExcluding("RequestPath like '%/health%' or RequestPath like '%/alive%' or RequestPath like '%/liveness%' or RequestPath like '%/readiness%'")
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", environment.EnvironmentName);

        if (isDevelopment)
        {
            loggerConfiguration.WriteTo.Async(a => a.Debug());
        }

        if (applicationName.Contains("API", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration.GetConnectionString("postgresdb") ?? throw new InvalidOperationException();
            // Add 'Include Error Detail' to see full PostgreSQL error messages
            if (!connectionString.Contains("Include Error Detail", StringComparison.OrdinalIgnoreCase))
            {
                connectionString += ";Include Error Detail=true";
            }

            loggerConfiguration.WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly("SourceContext = 'Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware'")
                .WriteTo.PostgreSQL(connectionString, "Requests", columnOptions, needAutoCreateTable: false, schemaName: "audit", useCopy: true, queueLimit: 3000, batchSizeLimit: 40, period: new TimeSpan(0, 0, 10), formatProvider: null));
        }

        // Always add console sink wrapped in async
        loggerConfiguration.WriteTo.Async(a => a.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Application}] {Message:lj} | TraceId: {TraceId} | Tenant: {TenantPath} | User: {UserId}{NewLine}{Exception}"));

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

    private sealed class GuidColumnWriter() : ColumnWriterBase(NpgsqlDbType.Uuid)
    {
        public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null) => Guid.NewGuid();
    }

    private sealed class TenantPathColumnWriter() : ColumnWriterBase(NpgsqlDbType.LTree)
    {
        public override object? GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null)
        {
            if (!logEvent.Properties.TryGetValue("TenantPath", out var propertyValue))
            {
                return DBNull.Value;
            }

            var tenant = propertyValue?.ToString()?.Trim('"');

            // Return DBNull.Value for null or empty strings to ensure PostgreSQL treats it as NULL
            // This prevents inserting empty ltree values '()' which would violate foreign key constraints
            return string.IsNullOrWhiteSpace(tenant) ? DBNull.Value : tenant;
        }
    }
}
