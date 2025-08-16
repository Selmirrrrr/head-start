using Ardalis.GuardClauses;
using CorrelationId.DependencyInjection;
using HeadStart.SharedKernel.Enrichers;
using Microsoft.Extensions.DependencyInjection;

namespace HeadStart.SharedKernel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedKernelServices(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddScoped<CorrelationIdEnricher>();
        services.AddDefaultCorrelationId(cfg =>
        {
            cfg.IncludeInResponse = true;
            cfg.UpdateTraceIdentifier = true;
            cfg.AddToLoggingScope = true;
            cfg.CorrelationIdGenerator = () => Guid.NewGuid().ToString();
        });

        return services;
    }
}
