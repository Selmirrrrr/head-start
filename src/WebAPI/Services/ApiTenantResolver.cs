using HeadStart.SharedKernel.Tenants;
using Microsoft.Extensions.Logging;

namespace HeadStart.WebAPI.Services;

public class ApiTenantResolver : ITenantResolver
{
    private readonly ILogger<ApiTenantResolver> _logger;
    private readonly IConfiguration _configuration;

    public ApiTenantResolver(
        ILogger<ApiTenantResolver> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task<TenantInfo?> ResolveAsync(HttpContext context)
    {
        // First, check for tenant header from BFF
        if (context.Request.Headers.TryGetValue(TenantConstants.TenantCodeHeader, out var tenantCodeHeader))
        {
            var tenantCode = tenantCodeHeader.ToString();
            if (!string.IsNullOrEmpty(tenantCode))
            {
                _logger.LogDebug("Resolved tenant code from header: {TenantCode}", tenantCode);
                return Task.FromResult<TenantInfo?>(new TenantInfo
                {
                    TenantCode = tenantCode,
                    TenantPath = tenantCode
                });
            }
        }

        // For development: check query parameter
        if (context.Request.Query.TryGetValue("tenant", out var tenantQuery))
        {
            var tenantCode = tenantQuery.ToString();
            if (!string.IsNullOrEmpty(tenantCode))
            {
                _logger.LogDebug("Resolved tenant code from query parameter: {TenantCode}", tenantCode);
                return Task.FromResult<TenantInfo?>(new TenantInfo
                {
                    TenantCode = tenantCode,
                    TenantPath = tenantCode
                });
            }
        }

        _logger.LogDebug("No tenant information found in request");
        return Task.FromResult<TenantInfo?>(null);
    }
}