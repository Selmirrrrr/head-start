using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class User
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public required string Nom { get; set; }

    public required string Prenom { get; set; }

    public bool IsDarkMode { get; set; }

    public string LanguageCode { get; set; } = "fr";

    public Tenant? LastSelectedTenant { get; set; }
    public LTree? LastSelectedTenantPath { get; set; }

    public ICollection<UserTenantRole> UserTenantRoles { get; set; } = new List<UserTenantRole>();
}

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

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

        builder.Property(u => u.IsDarkMode)
            .HasDefaultValue(false);

        builder.Property(u => u.LanguageCode)
            .HasMaxLength(5)
            .HasDefaultValue("fr")
            .IsRequired();

        builder.Property(u => u.LastSelectedTenantPath)
            .HasColumnType("ltree")
            .IsRequired(false);

        builder.HasOne(u => u.LastSelectedTenant)
            .WithMany()
            .HasForeignKey(u => u.LastSelectedTenantPath)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
