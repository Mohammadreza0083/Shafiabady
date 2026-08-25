using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.GuestName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.GuestEmail)
            .HasMaxLength(200);

        builder.Property(c => c.Text)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.Status)
            .IsRequired();

        // Recursive self-referencing relationship (Restrict prevents cascade cycles in SQL Server)
        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.ParentCommentId);
        builder.HasIndex(c => c.ArticleId);
        builder.HasIndex(c => c.BookId);
        builder.HasIndex(c => c.CourseId);
    }
}
