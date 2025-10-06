using HeadStart.WebAPI.Data.Models;
using HeadStart.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data;

public class HeadStartDbContext(
    DbContextOptions<HeadStartDbContext> options,
    CurrentUserService? currentUserService = null) : DbContext(options)
{
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Utilisateur> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Droit> UserTenantRoles { get; set; }
    public DbSet<AuditTrail> AuditTrails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure ltree extension is available for hierarchical data
        modelBuilder.HasPostgresExtension("ltree");

        modelBuilder.ApplyConfiguration(new TenantEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RoleEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UserTenantRoleEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new AuditTrailEntityTypeConfiguration());

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
        var auditableEntries = ChangeTracker.Entries<IAuditable>();
        var now = DateTime.UtcNow;
        var userId = currentUserService?.IsAuthenticated == true ? currentUserService.UserId : (Guid?)null;

        // Update audit fields
        foreach (var entry in auditableEntries)
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

        // Capture audit trail
        var auditTrails = await CreateAuditTrailsAsync(userId, now);

        var result = await base.SaveChangesAsync(cancellationToken);

        // Save audit trails in a separate operation to avoid circular references
        if (auditTrails.Count > 0)
        {
            await AuditTrails.AddRangeAsync(auditTrails, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private Task<List<AuditTrail>> CreateAuditTrailsAsync(Guid? userId, DateTime dateUtc)
    {
        var auditTrails = new List<AuditTrail>();

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditTrail
                && (e.State == EntityState.Added
                    || e.State == EntityState.Modified
                    || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var auditTrail = new AuditTrail
            {
                Id = Guid.NewGuid(),
                PrimaryKey = entry.Properties
                    .FirstOrDefault(p => p.Metadata.IsPrimaryKey())
                    ?.CurrentValue?.ToString(),
                UserId = userId,
                DateUtc = dateUtc,
                EntityName = entry.Metadata.ClrType.Name,
                Type = entry.State switch
                {
                    EntityState.Added => TrailType.Create,
                    EntityState.Modified => TrailType.Update,
                    EntityState.Deleted => TrailType.Delete,
                    _ => TrailType.None
                }
            };

            // Get property changes
            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditTrail.NewValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Modified:
                    {
                        if (property.IsModified)
                        {
                            auditTrail.OldValues[propertyName] = property.OriginalValue;
                            auditTrail.NewValues[propertyName] = property.CurrentValue;
                            auditTrail.ChangedColumns.Add(propertyName);
                        }

                        break;
                    }
                    case EntityState.Deleted:
                        auditTrail.OldValues[propertyName] = property.OriginalValue;
                        break;
                }
            }

            auditTrails.Add(auditTrail);
        }

        return Task.FromResult(auditTrails);
    }
}
