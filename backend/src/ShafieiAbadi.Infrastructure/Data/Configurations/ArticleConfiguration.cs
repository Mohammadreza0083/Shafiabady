using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Data.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(a => a.Slug)
            .IsUnique();

        builder.Property(a => a.Summary)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.Content)
            .IsRequired();

        builder.Property(a => a.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.AuthorName)
            .IsRequired()
            .HasMaxLength(200)
            .HasDefaultValue("دکتر محمد حسین شفیع‌آبادی");

        builder.Property(a => a.Tags)
            .HasMaxLength(500);

        builder.HasOne(a => a.Category)
            .WithMany(c => c.Articles)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Comments)
            .WithOne(c => c.Article)
            .HasForeignKey(c => c.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
