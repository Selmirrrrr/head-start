using Serilog.Core;
using Serilog.Events;

namespace HeadStart.SharedKernel.Enrichers;

public abstract class InstanceEnricher : ILogEventEnricher
{
    private readonly Guid _id = Guid.CreateVersion7();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        => logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("InstanceId", _id));
}
