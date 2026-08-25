using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(b => b.Slug)
            .IsUnique();

        builder.Property(b => b.Subtitle)
            .HasMaxLength(300);

        builder.Property(b => b.Description)
            .IsRequired();

        builder.Property(b => b.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.Publisher)
            .HasMaxLength(200);

        builder.Property(b => b.PublishYear)
            .HasMaxLength(50);

        builder.Property(b => b.ISBN)
            .HasMaxLength(50);

        builder.Property(b => b.ExternalBuyLink)
            .HasMaxLength(1000);

        builder.Property(b => b.DigitalDownloadLink)
            .HasMaxLength(1000);

        builder.HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Comments)
            .WithOne(c => c.Book)
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
