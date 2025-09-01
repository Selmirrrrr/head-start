using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class UserTenantRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public LTree TenantPath { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UserTenantRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserTenantRole>
{
    public void Configure(EntityTypeBuilder<UserTenantRole> builder)
    {
        builder.ToTable("UserTenantRoles");

        builder.HasKey(utr => new { utr.UserId, utr.TenantPath, utr.RoleId });

        builder.HasOne(utr => utr.User)
            .WithMany(u => u.UserTenantRoles)
            .HasForeignKey(utr => utr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(utr => utr.Tenant)
            .WithMany()
            .HasForeignKey(utr => utr.TenantPath)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(utr => utr.Role)
            .WithMany(r => r.UserTenantRoles)
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