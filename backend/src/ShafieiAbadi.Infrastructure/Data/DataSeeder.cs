using System.Security.Cryptography;
using System.Text;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.AdminUsers.Any())
            return;

        // Seed Admin User
        var admin = new AdminUser
        {
            Username = "admin",
            Email = "admin@shafieiabadi.ir",
            FullName = "دکتر محمد حسین شفیع‌آبادی",
            PasswordHash = HashPassword("Admin@123")
        };
        context.AdminUsers.Add(admin);

        // Seed Category
        var catArchitecture = new Category
        {
            Title = "معماری نرم‌افزار و مهندسی داده",
            Slug = "software-architecture",
            Description = "مقالات و مباحث تخصصی کلین آرکیتکچر، میکروسرویس و DDD",
            DisplayOrder = 1
        };
        var catAI = new Category
        {
            Title = "هوش مصنوعی و یادگیری ماشین",
            Slug = "ai-machine-learning",
            Description = "مباحث روز یادگیری عمیق، پردازش داده و بینایی ماشین",
            DisplayOrder = 2
        };
        context.Categories.AddRange(catArchitecture, catAI);

        // Seed Article
        var article = new Article
        {
            Title = "راهنمای جامع پیاده‌سازی معماری تمیز (Clean Architecture) در دات‌نت ۹",
            Slug = "clean-architecture-dotnet-9",
            Summary = "بررسی عملی اصول معماری تمیز، جداسازی لایه‌ها، الگوی ریپازیتوری و پیاده‌سازی تست‌های واحد بر اساس DDD",
            Content = "<p>در این مقاله به بررسی تفصیلی معماری تمیز و پیاده‌سازی آن با رویکرد Domain-Driven Design می‌پردازیم.</p>",
            ThumbnailUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=1200&q=80",
            AuthorName = "دکتر محمد حسین شفیع‌آبادی",
            ReadTimeMinutes = 8,
            Tags = "Clean Architecture, .NET 9, DDD, C#",
            Category = catArchitecture,
            ViewsCount = 1420
        };
        context.Articles.Add(article);

        // Seed Book
        var book = new Book
        {
            Title = "طراحی سیستم‌های توزیع‌شده با رویکرد میکروسرویس",
            Slug = "distributed-systems-microservices",
            Subtitle = "اصول، الگوها و چالش‌های معماری سرویس‌گرا در عمل",
            Description = "کتابی جامع برای مهندسان نرم‌افزار و معماران ارشد سیستم به تالیف دکتر محمد حسین شفیع‌آبادی.",
            ThumbnailUrl = "https://images.unsplash.com/photo-1532012164546-f432f2e3777a?auto=format&fit=crop&w=800&q=80",
            Publisher = "انتشارات دانشگاهی",
            PublishYear = "۱۴۰۲",
            PagesCount = 420,
            ISBN = "978-600-12345-6-7",
            ExternalBuyLink = "https://example.com/buy-book",
            DigitalDownloadLink = "https://example.com/download-sample.pdf",
            Category = catArchitecture,
            ViewsCount = 890
        };
        context.Books.Add(book);

        // Seed Course
        var course = new Course
        {
            Title = "دوره جامع معماری میکروسرویس و طراحی سیستم‌های مقیاس‌پذیر",
            Slug = "microservices-scalable-systems",
            Description = "آموزش گام به گام طراحی میکروسرویس‌ها با دات نت، ارتباطات ناهمگام، RabbitMQ و داکر.",
            ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&w=1200&q=80",
            AparatVideoUrl = "https://www.aparat.com/v/sample123",
            Level = "پیشرفته",
            Duration = "۳۲ ساعت",
            LessonsCount = 48,
            OriginalPrice = 6_500_000,
            DiscountedPrice = 4_550_000,
            DiscountPercentage = 30,
            Category = catArchitecture,
            ViewsCount = 3150
        };
        context.Courses.Add(course);

        await context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
