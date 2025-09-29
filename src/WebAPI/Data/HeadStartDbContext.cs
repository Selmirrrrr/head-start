using HeadStart.WebAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HeadStart.WebAPI.Data;

public class HeadStartDbContext : DbContext
{
    public HeadStartDbContext(DbContextOptions<HeadStartDbContext> options) : base(options)
    {
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
    }
}
