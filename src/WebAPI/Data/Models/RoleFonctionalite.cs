using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class RoleFonctionalite : IAuditable
{
    public Guid RoleId { get; init; }
    public Role Role { get; init; } = null!;

    public LTree FonctionaliteCode { get; init; }
    public Fonctionalite Fonctionalite { get; init; } = null!;

    // IAuditable
    public Audit Audit { get; set; } = new Audit();
}

public class RoleFonctionaliteEntityTypeConfiguration : IEntityTypeConfiguration<RoleFonctionalite>
{
    public void Configure(EntityTypeBuilder<RoleFonctionalite> builder)
    {
        builder.ToTable("RoleFonctionnalites");

        builder.HasKey(rf => new { rf.RoleId, rf.FonctionaliteCode });

        builder.Property(rf => rf.FonctionaliteCode)
            .HasColumnType("ltree")
            .IsRequired();

        builder.HasOne(rf => rf.Role)
            .WithMany(r => r.RoleFonctionnalites)
            .HasForeignKey(rf => rf.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rf => rf.Fonctionalite)
            .WithMany(f => f.RoleFonctionnalites)
            .HasForeignKey(rf => rf.FonctionaliteCode)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rf => new { rf.RoleId, rf.FonctionaliteCode });
        builder.HasIndex(rf => rf.RoleId);
        builder.HasIndex(rf => rf.FonctionaliteCode);
    }
}
