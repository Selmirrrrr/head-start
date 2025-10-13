using Ardalis.GuardClauses;
using HeadStart.SharedKernel.Enrichers;
using Microsoft.Extensions.DependencyInjection;

namespace HeadStart.SharedKernel.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedKernelServices(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddScoped<InstanceEnricher>();

        return services;
    }
}
