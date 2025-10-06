using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class Tenant : IAuditable
{
    public required LTree Code { get; set; }

    public required string Name { get; set; }

    public ICollection<Droit> Droits { get; set; } = new List<Droit>();

    public Audit Audit { get; set; } = new Audit();
}

public class TenantEntityTypeConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasKey(t => t.Code);

        builder.Property(t => t.Code)
            .HasColumnType("ltree")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(250)
            .IsRequired();
    }
}
