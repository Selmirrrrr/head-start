using Microsoft.Extensions.Logging;

namespace HeadStart.SharedKernel.Tenants;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;
    private readonly ITenantResolver _tenantResolver;

    public TenantMiddleware(
        RequestDelegate next,
        ILogger<TenantMiddleware> logger,
        ITenantResolver tenantResolver)
    {
        _next = next;
        _logger = logger;
        _tenantResolver = tenantResolver;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantInfo = await _tenantResolver.ResolveAsync(context);

        if (tenantInfo != null)
        {
            context.Items[TenantConstants.TenantInfoKey] = tenantInfo;

            // Add tenant information to logging context
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TenantCode"] = tenantInfo.TenantCode,
                ["TenantPath"] = tenantInfo.TenantPath
            }))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }
}

public static class TenantConstants
{
    public const string TenantInfoKey = "TenantInfo";
    public const string TenantCodeHeader = "X-Tenant-Code";
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}