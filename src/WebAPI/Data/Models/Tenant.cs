using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class Tenant
{
    [Key]
    public required LTree Path { get; set; }

    public required string Name { get; set; }
}

public class TenantEntityTypeConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasIndex(t => t.Path).IsUnique();
        builder.HasKey(t => t.Path);

        builder.Property(t => t.Path)
            .HasColumnType("ltree")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(250)
            .IsRequired();

    }
}
