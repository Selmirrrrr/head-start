using Serilog.Core;
using Serilog.Events;

namespace HeadStart.SharedKernel.Enrichers;

public sealed class InstanceEnricher : ILogEventEnricher
{
    private readonly Guid _id;

    public InstanceEnricher()
    {
        _id = Guid.NewGuid();
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        => logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("InstanceId", _id));
}