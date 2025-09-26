namespace HeadStart.SharedKernel.Tenants;

public interface ITenantContextAccessor
{
    TenantInfo? CurrentTenant { get; }
}

public class TenantContextAccessor : ITenantContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TenantInfo? CurrentTenant
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            if (httpContext.Items.TryGetValue(TenantConstants.TenantInfoKey, out var tenantInfo))
            {
                return tenantInfo as TenantInfo;
            }

            return null;
        }
    }
}