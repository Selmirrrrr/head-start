using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public class AuditRequest : IMayHaveTenant
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public Utilisateur? User { get; init; }
    public Guid? ImpersonatedByUserId { get; init; }
    public Utilisateur? ImpersonatedByUser { get; init; }
    public DateTime DateUtc { get; init; } = DateTime.UtcNow;
    public required string RequestId { get; init; }
    public required string RequestPath { get; init; }
    public required string RequestMethod { get; init; }
    public string? RequestBody { get; init; }
    public int? ResponseStatusCode { get; init; }

    // IMayHaveTenant
    public LTree? TenantPath { get; set; }
    public Tenant? Tenant { get; set; }
}

public class AuditRequestEntityTypeConfiguration : IEntityTypeConfiguration<AuditRequest>
{
    public void Configure(EntityTypeBuilder<AuditRequest> builder)
    {
        builder.ToTable("Requests", "audit");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.DateUtc)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.RequestId)
            .IsRequired();

        builder.Property(e => e.RequestPath)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(e => e.RequestMethod)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.RequestBody)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(e => e.ResponseStatusCode)
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ImpersonatedByUser)
            .WithMany()
            .HasForeignKey(e => e.ImpersonatedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantPath)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
