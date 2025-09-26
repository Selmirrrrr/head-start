using HeadStart.SharedKernel.Tenants;
using HeadStart.WebAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data;

public class HeadStartDbContext : DbContext
{
    private readonly ITenantContextAccessor? _tenantContextAccessor;

    public HeadStartDbContext(DbContextOptions<HeadStartDbContext> options) : base(options)
    {
    }

    public HeadStartDbContext(DbContextOptions<HeadStartDbContext> options, ITenantContextAccessor tenantContextAccessor) : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Utilisateur> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Droit> UserTenantRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RoleEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserTenantRoleEntityTypeConfiguration());

        // Apply tenant filters for entities implementing IHaveTenant
        if (_tenantContextAccessor != null)
        {
            var currentTenant = _tenantContextAccessor.CurrentTenant;
            if (currentTenant != null)
            {
                var tenantPath = new LTree(currentTenant.TenantPath);

                // Add query filters for tenant-scoped entities
                // Note: We'll need to add these filters to specific entities as they implement IHaveTenant
            }
        }
    }
}
