using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HeadStart.SharedKernel.Tenants;

public class SubdomainTenantResolver : ITenantResolver
{
    private readonly ILogger<SubdomainTenantResolver> _logger;
    private readonly IConfiguration _configuration;

    public SubdomainTenantResolver(
        ILogger<SubdomainTenantResolver> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task<TenantInfo?> ResolveAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        _logger.LogDebug("Resolving tenant for host: {Host}", host);

        var tenantCode = ExtractTenantCodeFromHost(host);

        if (string.IsNullOrEmpty(tenantCode))
        {
            _logger.LogDebug("No tenant code found in subdomain");
            return Task.FromResult<TenantInfo?>(null);
        }

        _logger.LogInformation("Resolved tenant code: {TenantCode} from host: {Host}", tenantCode, host);

        return Task.FromResult<TenantInfo?>(new TenantInfo
        {
            TenantCode = tenantCode,
            TenantPath = tenantCode
        });
    }

    private string? ExtractTenantCodeFromHost(string host)
    {
        var baseDomain = _configuration["Tenancy:BaseDomain"] ?? "headstart.ch";

        // For local development, support localhost with port
        if (host.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.StartsWith("127.0.0.1") ||
            host.StartsWith("::1"))
        {
            // In development, we can use a header or query parameter
            return null;
        }

        // Remove port if present
        var hostWithoutPort = host.Split(':')[0];

        // Check if host ends with base domain
        if (!hostWithoutPort.EndsWith(baseDomain, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Host {Host} does not match base domain {BaseDomain}", host, baseDomain);
            return null;
        }

        // Extract subdomain
        var subdomainLength = hostWithoutPort.Length - baseDomain.Length - 1; // -1 for the dot
        if (subdomainLength <= 0)
        {
            // No subdomain (e.g., headstart.ch)
            return null;
        }

        var subdomain = hostWithoutPort.Substring(0, subdomainLength);

        // Validate subdomain (alphanumeric and hyphens only)
        if (!IsValidTenantCode(subdomain))
        {
            _logger.LogWarning("Invalid tenant code format: {TenantCode}", subdomain);
            return null;
        }

        return subdomain.ToLowerInvariant();
    }

    private static bool IsValidTenantCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        // Tenant code should be alphanumeric with hyphens,
        // start and end with alphanumeric
        return System.Text.RegularExpressions.Regex.IsMatch(
            code,
            @"^[a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?$");
    }
}