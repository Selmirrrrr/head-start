using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class Fonctionalite : IMayHaveTenant, IAuditable
{
    public required LTree Code { get; init; } = null!;
    public required string Libelle { get; init; } = null!;
    public required Dictionary<string, string> LibelleTrads { get; init; }

    public IReadOnlyCollection<RoleFonctionalite> RoleFonctionnalites { get; init; } = new List<RoleFonctionalite>();

    // IMayHaveTenant
    public LTree? TenantPath { get; set; }
    public Tenant? Tenant { get; set; }

    // IAuditable
    public Audit Audit { get; set; } = new();
}

public class FonctionaliteEntityTypeConfiguration : IEntityTypeConfiguration<Fonctionalite>
{
    public void Configure(EntityTypeBuilder<Fonctionalite> builder)
    {
        builder.ToTable("Fonctionnalites");

        builder.HasKey(f => f.Code);
        builder.HasIndex(t => t.Code).IsUnique();

        builder.Property(t => t.Code)
            .HasColumnType("ltree")
            .IsRequired();

        builder.Property(f => f.Libelle)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.LibelleTrads)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne(f => f.Tenant)
            .WithMany()
            .HasForeignKey(f => f.TenantPath)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.RoleFonctionnalites)
            .WithOne(rf => rf.Fonctionalite)
            .HasForeignKey(rf => rf.FonctionaliteCode)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.Code);
        builder.HasIndex(f => f.TenantPath);
    }
}
