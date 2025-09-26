using HeadStart.SharedKernel.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HeadStart.WebAPI.Data;

public class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public TenantSaveChangesInterceptor(ITenantContextAccessor tenantContextAccessor)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyTenantInfo(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantInfo(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantInfo(DbContext? context)
    {
        if (context == null)
            return;

        var currentTenant = _tenantContextAccessor.CurrentTenant;
        if (currentTenant == null)
            return;

        var tenantPath = new LTree(currentTenant.TenantPath);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is IHaveTenant haveTenant)
                {
                    haveTenant.TenantPath = tenantPath;
                }
                else if (entry.Entity is IMayHaveTenant mayHaveTenant && mayHaveTenant.TenantPath == null)
                {
                    mayHaveTenant.TenantPath = tenantPath;
                }
            }
        }
    }
}