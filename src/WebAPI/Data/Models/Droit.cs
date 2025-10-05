using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class Droit
{
    private Droit() { }

    public Guid UserId { get; init; }
    public Utilisateur Utilisateur { get; init; } = null!;

    public LTree TenantPath { get; init; }
    public Tenant Tenant { get; init; } = null!;

    public Guid RoleId { get; init; }
    public Role Role { get; init; } = null!;

    public DateTime DateDebutValidite { get; init; }
    public DateTime DateFinValidite { get; private set; }

    public static Droit New(Guid userId, LTree tenantPath, Guid roleId, DateTime dateDebutValidite, DateTime dateFinValidite)
    {
        Guard.Against.Expression(x => dateDebutValidite > x, dateFinValidite, "La date de fin de validité doit être après la date de début de validité");
        return new Droit
        {
            UserId = userId,
            TenantPath = tenantPath,
            RoleId = roleId,
            DateDebutValidite = dateDebutValidite,
            DateFinValidite = dateFinValidite
        };
    }

    public static Droit New(Guid userId, LTree tenantPath, Guid roleId)
    {
        return New(userId, tenantPath, roleId, DateTime.UtcNow, DateTime.UtcNow.AddYears(1));

    }

    public static Droit New(Guid userId, LTree tenantPath, Guid roleId, DateTime dateDebutValidite)
    {
        return New(userId, tenantPath, roleId, dateDebutValidite, dateDebutValidite.AddYears(1));
    }

    public void Update(DateTime dateFinValidite)
    {
        Guard.Against.Expression(x => DateDebutValidite > x, dateFinValidite, "La date de fin de validité doit être après la date de début de validité");
        DateFinValidite = dateFinValidite;
    }
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

        builder.Property(utr => utr.DateDebutValidite)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(utr => utr.DateFinValidite)
            .IsRequired();

        builder.HasIndex(utr => new { utr.UserId, utr.TenantPath });
        builder.HasIndex(utr => utr.UserId);
        builder.HasIndex(utr => utr.TenantPath);
        builder.HasIndex(utr => utr.RoleId);
    }
}
