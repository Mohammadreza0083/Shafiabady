using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Data.Configurations;

public class ViewLogConfiguration : IEntityTypeConfiguration<ViewLog>
{
    public void Configure(EntityTypeBuilder<ViewLog> builder)
    {
        builder.ToTable("ViewLogs");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.IpAddress)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.UserAgent)
            .HasMaxLength(500);

        builder.Property(v => v.EntityType)
            .IsRequired();

        builder.Property(v => v.EntityId)
            .IsRequired();

        builder.Property(v => v.ViewedAt)
            .IsRequired();

        // Index for fast lookup in 24-hour IP view check
        builder.HasIndex(v => new { v.IpAddress, v.EntityType, v.EntityId, v.ViewedAt });
    }
}
