using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class Role : IMayHaveTenant
{
    public Guid Id { get; set; }

    public required string Code { get; set; }
    public required Dictionary<string, string> CodeTrads { get; set; }

    public string? Description { get; set; }

    public LTree? TenantPath { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public ICollection<Droit> Droits { get; set; } = new List<Droit>();
}

public class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Code);

        builder.Property(r => r.CodeTrads)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.HasOne(utr => utr.Tenant)
            .WithMany()
            .HasForeignKey(utr => utr.TenantPath)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
