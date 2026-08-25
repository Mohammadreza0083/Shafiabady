using Microsoft.EntityFrameworkCore;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the Dr. Shafiei Abadi platform.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ViewLog> ViewLogs => Set<ViewLog>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
