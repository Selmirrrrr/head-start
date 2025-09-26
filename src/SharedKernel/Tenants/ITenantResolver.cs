namespace HeadStart.SharedKernel.Tenants;

public interface ITenantResolver
{
    Task<TenantInfo?> ResolveAsync(HttpContext context);
}

public class TenantInfo
{
    public required string TenantCode { get; init; }
    public required string TenantPath { get; init; }
    public string? TenantName { get; init; }
}