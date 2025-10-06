using HeadStart.WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class Utilisateur : IAuditable
{
    public Guid Id { get; set; }
    public Guid IdpId { get; set; }

    public required string Email { get; set; }

    public required string Nom { get; set; }

    public required string Prenom { get; set; }

    public bool DarkMode { get; set; }

    public string LanguageCode { get; set; } = "fr";

    public Tenant? DernierTenantSelectionne { get; set; }
    public LTree? DernierTenantSelectionneId { get; set; }

    public ICollection<Droit> Droits { get; set; } = new List<Droit>();

    public Audit Audit { get; set; } = new Audit();
}

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<Utilisateur>
{
    public void Configure(EntityTypeBuilder<Utilisateur> builder)
    {
        builder.ToTable("Utilisateurs");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.IdpId)
            .IsRequired();

        builder.HasIndex(u => u.IdpId)
            .IsUnique();

        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Nom)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Prenom)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.DarkMode)
            .HasDefaultValue(false);

        builder.Property(u => u.LanguageCode)
            .HasMaxLength(5)
            .HasDefaultValue("fr")
            .IsRequired();

        builder.Property(u => u.DernierTenantSelectionneId)
            .HasColumnType("ltree")
            .IsRequired(false);

        builder.HasOne(u => u.DernierTenantSelectionne)
            .WithMany()
            .HasForeignKey(u => u.DernierTenantSelectionneId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
