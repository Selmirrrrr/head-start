using CorrelationId.Abstractions;
using Serilog.Core;
using Serilog.Events;

namespace HeadStart.SharedKernel.Enrichers;

/// <summary>
/// Enriches the log event with a 'CorrelationId' of the currently processed request.
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public CorrelationIdEnricher(ICorrelationContextAccessor correlationContextAccessor)
    {
        this._correlationContextAccessor = correlationContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = _correlationContextAccessor.CorrelationContext.CorrelationId;
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("CorrelationId", correlationId));
    }
}
