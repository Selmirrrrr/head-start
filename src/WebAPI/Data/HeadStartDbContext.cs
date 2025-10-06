using HeadStart.WebAPI.Data.Models;
using HeadStart.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data;

public class HeadStartDbContext : DbContext
{
    private readonly CurrentUserService? _currentUserService;

    public HeadStartDbContext(
        DbContextOptions<HeadStartDbContext> options,
        CurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Utilisateur> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Droit> UserTenantRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure ltree extension is available for hierarchical data
        modelBuilder.HasPostgresExtension("ltree");

        modelBuilder.ApplyConfiguration(new TenantEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RoleEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserTenantRoleEntityTypeConfiguration());

        // Configure audit records for all auditable entities
        ConfigureAuditRecords(modelBuilder);
    }

    private static void ConfigureAuditRecords(ModelBuilder modelBuilder)
    {
        var auditableEntityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(IAuditable).IsAssignableFrom(e.ClrType))
            .ToList();

        foreach (var entityType in auditableEntityTypes)
        {
            modelBuilder.Entity(entityType.ClrType)
                .OwnsOne(typeof(Audit), nameof(IAuditable.Audit));
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditable>();
        var now = DateTime.UtcNow;
        var userId = _currentUserService?.IsAuthenticated == true ? _currentUserService.UserId : (Guid?)null;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Audit.CreeLe = now;
                entry.Entity.Audit.CreePar = userId;
                entry.Entity.Audit.ModifieLe = now;
                entry.Entity.Audit.ModifiePar = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.Audit.ModifieLe = now;
                entry.Entity.Audit.ModifiePar = userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
