using HeadStart.SharedKernel.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace HeadStart.WebAPI.Data;

public class TenantDbContextInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public TenantDbContextInterceptor(ITenantContextAccessor tenantContextAccessor)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await SetTenantParameterAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetTenantParameterAsync(connection, CancellationToken.None).GetAwaiter().GetResult();
        base.ConnectionOpened(connection, eventData);
    }

    private async Task SetTenantParameterAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var currentTenant = _tenantContextAccessor.CurrentTenant;
        if (currentTenant != null)
        {
            // Set PostgreSQL session variable for row-level security
            using var command = connection.CreateCommand();
            command.CommandText = $"SET app.current_tenant_path = '{currentTenant.TenantPath}'";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}