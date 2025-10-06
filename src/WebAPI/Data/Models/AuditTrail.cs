using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeadStart.WebAPI.Data.Models;

public enum TrailType : byte
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3
}

public class AuditTrail
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public Utilisateur? User { get; init; }
    public TrailType Type { get; init; }
    public DateTime DateUtc { get; init; }
    public required string EntityName { get; init; }
    public string? PrimaryKey { get; init; }
    public Dictionary<string, object?> OldValues { get; init; } = new();
    public Dictionary<string, object?> NewValues { get; init; } = new();
    public List<string> ChangedColumns { get; init; } = new();
}

public class AuditTrailEntityTypeConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("AuditTrails");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.DateUtc)
            .IsRequired();

        builder.Property(a => a.EntityName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.PrimaryKey)
            .HasMaxLength(255);

        builder.Property(a => a.ChangedColumns)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.OldValues)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.NewValues)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.General)),
                v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, new JsonSerializerOptions(JsonSerializerDefaults.General))!)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.EntityName);
        builder.HasIndex(a => a.PrimaryKey);
        builder.HasIndex(a => a.DateUtc);
        builder.HasIndex(a => a.Type);
    }
}
