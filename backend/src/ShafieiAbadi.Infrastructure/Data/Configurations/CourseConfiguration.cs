using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.Property(c => c.Description)
            .IsRequired();

        builder.Property(c => c.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.AparatVideoUrl)
            .HasMaxLength(1000);

        builder.Property(c => c.Level)
            .HasMaxLength(100);

        builder.Property(c => c.Duration)
            .HasMaxLength(100);

        builder.Property(c => c.OriginalPrice)
            .HasPrecision(18, 2);

        builder.Property(c => c.DiscountedPrice)
            .HasPrecision(18, 2);

        builder.HasOne(c => c.Category)
            .WithMany(cat => cat.Courses)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Comments)
            .WithOne(com => com.Course)
            .HasForeignKey(com => com.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
