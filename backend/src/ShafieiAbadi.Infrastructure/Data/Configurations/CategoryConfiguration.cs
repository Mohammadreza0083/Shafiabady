using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(1000);
    }
}
