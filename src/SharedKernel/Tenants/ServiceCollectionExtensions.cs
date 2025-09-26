using Microsoft.Extensions.DependencyInjection;

namespace HeadStart.SharedKernel.Tenants;

public static class TenantServiceExtensions
{
    public static IServiceCollection AddMultiTenancySupport(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantResolver, SubdomainTenantResolver>();
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

        return services;
    }
}