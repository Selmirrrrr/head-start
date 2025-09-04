using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class Droit
{
    public Guid UserId { get; set; }
    public Utilisateur Utilisateur { get; set; } = null!;

    public LTree TenantPath { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UserTenantRoleEntityTypeConfiguration : IEntityTypeConfiguration<Droit>
{
    public void Configure(EntityTypeBuilder<Droit> builder)
    {
        builder.ToTable("Droits");

        builder.HasKey(utr => new { utr.UserId, utr.TenantPath, utr.RoleId });

        builder.HasOne(utr => utr.Utilisateur)
            .WithMany(u => u.Droits)
            .HasForeignKey(utr => utr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(utr => utr.Tenant)
            .WithMany(utr => utr.Droits)
            .HasForeignKey(utr => utr.TenantPath)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(utr => utr.Role)
            .WithMany(r => r.Droits)
            .HasForeignKey(utr => utr.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(utr => utr.TenantPath)
            .HasColumnType("ltree");

        builder.Property(utr => utr.AssignedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(utr => new { utr.UserId, utr.TenantPath });
        builder.HasIndex(utr => utr.TenantPath);
        builder.HasIndex(utr => utr.RoleId);
    }
}
