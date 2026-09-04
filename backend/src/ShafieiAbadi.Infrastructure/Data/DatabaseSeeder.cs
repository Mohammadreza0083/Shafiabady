using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Data;

/// <summary>
/// Service responsible for seeding the database with initial admin credentials and realistic mock data on startup.
/// </summary>
public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Executes the full database seeding workflow.
    /// Schema creation and migrations are handled in Program.cs via MigrateAsync().
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            var adminSeeded = await SeedAdminUserAsync();
            var (catCount, articleCount, bookCount, courseCount) = await SeedNonSensitiveEntitiesAsync();

            _logger.LogInformation(
                "Database seeding completed successfully. Summary: {AdminCount} admin user, {CatCount} categories, {ArticleCount} articles, {BookCount} books, {CourseCount} courses.",
                adminSeeded ? 1 : 0, catCount, articleCount, bookCount, courseCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task<bool> SeedAdminUserAsync()
    {
        // 1. Read Admin Credentials from Configuration
        var adminUsername = _configuration["AdminSeeding:Username"] ?? "admin";
        var adminEmail = _configuration["AdminSeeding:Email"] ?? "admin@shafieiabadi.ir";
        var rawPassword = _configuration["AdminSeeding:RawPassword"] ?? "Admin@123Password!";
        var fullName = _configuration["AdminSeeding:FullName"] ?? "دکتر محمد حسین شفیع‌آبادی";

        var adminExists = await _context.AdminUsers.AnyAsync(u => u.Username == adminUsername || u.Email == adminEmail);
        if (!adminExists)
        {
            _logger.LogInformation("Seeding default administrator '{Username}'...", adminUsername);
            var adminUser = new AdminUser
            {
                Id = Guid.NewGuid(),
                Username = adminUsername,
                Email = adminEmail,
                FullName = fullName,
                PasswordHash = _passwordHasher.HashPassword(rawPassword),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AdminUsers.AddAsync(adminUser);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully seeded default administrator user into SQL Server.");
            return true;
        }

        return false;
    }

    private async Task<(int catCount, int articleCount, int bookCount, int courseCount)> SeedNonSensitiveEntitiesAsync()
    {
        int seededCatCount = 0;
        int seededArticleCount = 0;
        int seededBookCount = 0;
        int seededCourseCount = 0;

        // 2. Seed Categories if empty
        Category catArch;
        Category catAI;
        Category catDistributed;
        Category catWeb;

        if (!await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Seeding initial categories into SQL Server...");

            catArch = new Category
            {
                Id = Guid.NewGuid(),
                Title = "معماری نرم‌افزار و مهندسی داده",
                Slug = "software-architecture",
                Description = "مباحث تخصصی پیرامون اصول Clean Architecture، Domain-Driven Design و الگوهای طراحی مقیاس‌پذیر",
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            };

            catAI = new Category
            {
                Id = Guid.NewGuid(),
                Title = "هوش مصنوعی و یادگیری ماشین",
                Slug = "ai-machine-learning",
                Description = "تکنیک‌ها و کاربردهای یادگیری عمیق، مدل‌های زبانی بزرگ و بینایی ماشین در سیستم‌های کاربردی",
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            };

            catDistributed = new Category
            {
                Id = Guid.NewGuid(),
                Title = "طراحی سیستم‌های توزیع‌شده و کلود",
                Slug = "distributed-cloud-systems",
                Description = "الگوهای میکروسرویس، سیستم‌های پیام‌محور، همگام‌سازی توزیع‌شده و زیرساخت‌های ابری",
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            };

            catWeb = new Category
            {
                Id = Guid.NewGuid(),
                Title = "توسعه وب مدرن و فریم‌ورک‌ها",
                Slug = "modern-web-development",
                Description = "توسعه رابط‌های کاربری مدرن با Astro.js، React، Tailwind CSS و تعامل بهینه با وب سرویس‌های RESTful",
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddRangeAsync(catArch, catAI, catDistributed, catWeb);
            await _context.SaveChangesAsync();
            seededCatCount = 4;
            _logger.LogInformation("Successfully seeded {CatCount} categories into SQL Server.", seededCatCount);
        }
        else
        {
            catArch = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "software-architecture") ?? await _context.Categories.FirstAsync();
            catAI = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "ai-machine-learning") ?? catArch;
            catDistributed = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "distributed-cloud-systems") ?? catArch;
            catWeb = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "modern-web-development") ?? catArch;
        }

        // 3. Seed Articles if empty (At least 10 articles for infinite scrolling testing)
        if (!await _context.Articles.AnyAsync())
        {
            _logger.LogInformation("Seeding initial articles into SQL Server...");

            var articles = new List<Article>
            {
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "راهنمای جامع پیاده‌سازی معماری تمیز (Clean Architecture) در دات‌نت ۹",
                    Slug = "clean-architecture-dotnet-9",
                    Summary = "بررسی عملی اصول معماری تمیز، جداسازی لایه‌ها، الگوی ریپازیتوری و پیاده‌سازی تست‌های واحد بر اساس DDD در جدیدترین نگارش دات‌نت.",
                    Content = "<p>در دنیای مهندسی نرم‌افزار مدرن، یکی از اساسی‌ترین نیازمندی‌های پروژه‌های بزرگ و سازمانی، قابلیت نگهداری (Maintainability) و آزمون‌پذیری (Testability) بالا است. معماری تمیز (Clean Architecture) با تکیه بر اصل وارونگی وابستگی‌ها این امکان را فراهم می‌سازد تا منطق کسب‌وکار بدون وابستگی به فریم‌ورک‌ها توسعه یابد.</p><p>در این راهنما به طور کامل به تشریح لایه‌های Domain، Application، Infrastructure و Web API می‌پردازیم.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 8,
                    Tags = "Clean Architecture, .NET 9, DDD, C#, Enterprise",
                    Category = catArch,
                    ViewsCount = 1420,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "اصول طراحی دامنه‌محور (Domain-Driven Design) در پروژه‌های سازمانی",
                    Slug = "domain-driven-design-in-practice",
                    Summary = "چگونه مدل‌های دامنه غنی (Rich Domain Models)، موجودیت‌ها، اشیای مقدار (Value Objects) و ریشه‌های مجتمع (Aggregate Roots) بسازیم؟",
                    Content = "<p>طراحی دامنه‌محور یا DDD صرفاً یک الگوی کدنویسی نیست، بلکه رویکردی عمیق برای هم‌راستاسازی مهندسی نرم‌افزار با مفاهیم کسب‌وکار است. با تعریف دقیق مرزهای محصور (Bounded Contexts) و زبان مشترک (Ubiquitous Language) می‌توان پیچیدگی نرم‌افزارهای مدرن را مهار کرد.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 12,
                    Tags = "DDD, Software Architecture, Domain Modeling, Microservices",
                    Category = catArch,
                    ViewsCount = 1890,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "تست‌نویسی با رویکرد TDD در سی‌شارپ: راهنمای عملی از صفر تا صد",
                    Slug = "test-driven-development-tdd-guide",
                    Summary = "آموزش گام‌به‌گام چرخه Red-Green-Refactor و نوشتن تست‌های واحد، یکپارچه‌سازی و ایزولاسیون وابستگی‌ها با Moq و FluentAssertions.",
                    Content = "<p>توسعه آزمون‌محور (Test-Driven Development) یکی از بنیادی‌ترین مهارت‌های مهندسان ارشد نرم‌افزار است. با TDD نه تنها از صحت عملکرد کد اطمینان حاصل می‌کنید، بلکه طراحی ماژولار و ساختار کد نیز به طور طبیعی بهبود می‌یابد.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1516259762381-22954d7d3ad2?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 6,
                    Tags = "TDD, Unit Testing, xUnit, Moq, C#",
                    Category = catArch,
                    ViewsCount = 950,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "الگوهای CQRS و Event Sourcing در سیستم‌های میکروسرویس",
                    Slug = "microservices-cqrs-event-sourcing",
                    Summary = "جداسازی مسیر خواندن و نوشتن، بهینه‌سازی توان عملیاتی سیستم‌های حجیم و ثبت وقایع تاریخی به عنوان منبع حقیقت داده‌ها.",
                    Content = "<p>در سیستم‌های پرترافیک و مقیاس‌پذیر، تفکیک مسئولیت‌های مربوط به تغییر وضعیت داده‌ها (Commands) از استعلام داده‌ها (Queries) کارایی سیستم را به حداکثر می‌رساند.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 10,
                    Tags = "CQRS, Event Sourcing, Microservices, Scalability",
                    Category = catDistributed,
                    ViewsCount = 2100,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-18)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "رندرینگ هایبرید و بهینه‌سازی پرفورمنس در فرانت‌اند با Astro.js",
                    Slug = "astro-js-hybrid-rendering-guide",
                    Summary = "بررسی جزایر معماری (Islands Architecture)، رندر سمت سرور SSR و دستیابی به نمره ۱۰۰ در ارزیابی Core Web Vitals با Astro.",
                    Content = "<p>فریم‌ورک Astro با معماری Islands امکان تولید صفحات با حجم جاوااسکریپت نزدیک به صفر را فراهم می‌کند و بهترین گزینه برای وب‌سایت‌های داده‌محور، بلاگ‌های تخصصی و پرتال‌های آموزشی است.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1507238691740-187a5b1d37b8?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 5,
                    Tags = "Astro, TypeScript, Frontend, Performance, SEO",
                    Category = catWeb,
                    ViewsCount = 760,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "بهینه‌سازی پیشرفته کوئری‌ها و کارایی در Entity Framework Core 9",
                    Slug = "high-performance-ef-core-optimizations",
                    Summary = "نکات کلیدی No-Tracking، Split Queries، Batch Updates، ایندکس‌گذاری دیتابیس و جلوگیری از مشکل N+1 در برنامه‌های دات‌نت.",
                    Content = "<p>شناخت نحوه تولید کوئری‌های SQL توسط EF Core و مدیریت صحیح کانتکست دیتابیس از ملزومات توسعه سرویس‌های دات‌نت با کارایی فوق‌العاده بالا است.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1544383835-bda2bc66a55d?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 9,
                    Tags = "EF Core 9, SQL Server, Performance, .NET, Database",
                    Category = catArch,
                    ViewsCount = 1630,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "طراحی ساختار امنیت و احراز هویت توکن‌محور با HttpOnly Cookies",
                    Slug = "secure-authentication-httponly-cookies",
                    Summary = "بررسی خطرات XSS در ذخیره‌سازی JWT در LocalStorage و راهکار امن انتقال توکن از طریق کوکی‌های محافظت‌شده در ASP.NET Core.",
                    Content = "<p>نگهداری توکن‌های اعتبارسنجی در LocalStorage مرورگرها می‌تواند نرم‌افزار را در معرض حملات سرقت شناسه (Token Theft via XSS) قرار دهد. استفاده از کوکی‌های HttpOnly با پرچم‌های Secure و SameSite بالاترین استاندارد امنیتی را تضمین می‌کند.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1563986768609-322da13575f3?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 7,
                    Tags = "Security, JWT, HttpOnly Cookies, Auth, Web Security",
                    Category = catArch,
                    ViewsCount = 1120,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "تکنیک‌های پیاده‌سازی سیستم‌های توزیع‌شده و کشینگ لایه‌بندی شده با Redis",
                    Slug = "distributed-caching-redis-patterns",
                    Summary = "راهکارهای Cache-Aside، Write-Through، جلوگیری از وقوع Cache Stampede و استراتژی‌های انقضای داده در سیستم‌های توزیع‌شده.",
                    Content = "<p>استفاده هوشمندانه از کش توزیع‌شده می‌تواند بار وارد بر پایگاه‌داده را تا بیش از ۹۰ درصد کاهش داده و پاسخگویی به میلیون‌ها درخواست در ثانیه را ممکن سازد.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 11,
                    Tags = "Redis, Distributed Caching, Scalability, High Availability",
                    Category = catDistributed,
                    ViewsCount = 1340,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "مهندسی پرامپت و ادغام مدل‌های زبانی بزرگ (LLM) در معماری‌های سازمانی",
                    Slug = "llm-integration-enterprise-architecture",
                    Summary = "معماری RAG (Retrieval-Augmented Generation)، برداری‌سازی متون، پایگاه‌های داده Vector و ساخت دستیارهای هوشمند اختصاصی.",
                    Content = "<p>همگرایی مدل‌های هوش مصنوعی زاینده با داده‌های اختصاصی سازمان‌ها از طریق پایگاه‌های داده برداری، چشم‌انداز جدیدی از سیستم‌های تصمیم‌یار خودکار را شکل داده است.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1677442136019-21780ecad995?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 14,
                    Tags = "AI, LLM, RAG, Machine Learning, Python, Semantic Search",
                    Category = catAI,
                    ViewsCount = 2850,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Article
                {
                    Id = Guid.NewGuid(),
                    Title = "استقرار ابری برنامه‌های دات‌نت و داکر با اصول CI/CD و Observability",
                    Slug = "docker-cloud-native-dotnet-observability",
                    Summary = "کانتینری‌سازی سرویس‌ها، تنظیمات لاگینگ ساختاریافته با Serilog و Grafana Loki، متریک‌های Prometheus و تریس توزیع‌شده با OpenTelemetry.",
                    Content = "<p>قابلیت مشاهده‌پذیری (Observability) در سیستم‌های Cloud Native ستون فقرات پایداری سرویس‌ها است. با ترکیب لاگ‌های ساختاریافته، متریک‌ها و تریس‌ها می‌توان اشکالات را در کسری از ثانیه ردیابی کرد.</p>",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1607799279861-4dd421887fb3?auto=format&fit=crop&w=1200&q=80",
                    AuthorName = "دکتر محمد حسین شفیع‌آبادی",
                    ReadTimeMinutes = 9,
                    Tags = "Docker, DevOps, CI/CD, Observability, Grafana Loki",
                    Category = catDistributed,
                    ViewsCount = 1750,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await _context.Articles.AddRangeAsync(articles);
            await _context.SaveChangesAsync();
            seededArticleCount = articles.Count;
            _logger.LogInformation("Database seeded successfully with {Count} articles.", seededArticleCount);
            _logger.LogInformation("Successfully seeded {Count} articles into SQL Server.", seededArticleCount);
        }

        // 4. Seed Books if empty (At least 3 books)
        if (!await _context.Books.AnyAsync())
        {
            _logger.LogInformation("Seeding initial academic books into SQL Server...");

            var books = new List<Book>
            {
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "راهنمای عملی معماری تمیز در اکوسیستم دات‌نت",
                    Slug = "hands-on-clean-architecture-dotnet",
                    Subtitle = "اصول، الگوها و پیاده‌سازی پروژه‌های بزرگ در دات‌نت ۹ با رویکرد DDD",
                    Description = "کتابی جامع و تخصصی برای معماران نرم‌افزار و برنامه‌نویسان ارشد سی‌شارپ که قصد دارند سیستم‌هایی مقیاس‌پذیر، ماژولار و کاملاً آزمون‌پذیر بر پایه اصول Clean Architecture و DDD خلق کنند.",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=800&q=80",
                    Publisher = "انتشارات دانش و فناوری",
                    PublishYear = "۱۴۰۲",
                    PagesCount = 384,
                    ISBN = "978-600-12345-1-1",
                    ExternalBuyLink = "https://example.com/buy-clean-arch",
                    DigitalDownloadLink = "https://example.com/download-sample.pdf",
                    Category = catArch,
                    ViewsCount = 1240,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "طراحی سیستم‌های توزیع‌شده با رویکرد میکروسرویس",
                    Slug = "mastering-system-design-persian",
                    Subtitle = "مفاهیم پیشرفته، مقیاس‌پذیری و چالش‌های معماری توزیع‌شده در عمل",
                    Description = "این اثر حاصل سال‌ها تجربه عملی در طراحی و پیاده‌سازی سیستم‌های در مقیاس بالا و آموزش به صدها مهندس نرم‌افزار است. با مطالعه این اثر، تفکر معمارانه و تصمیم‌گیری ساختاری شما ارتقا می‌یابد.",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&w=800&q=80",
                    Publisher = "انتشارات دانشگاهی",
                    PublishYear = "۱۴۰۳",
                    PagesCount = 456,
                    ISBN = "978-600-12345-2-2",
                    ExternalBuyLink = "https://example.com/buy-system-design",
                    DigitalDownloadLink = "https://example.com/sample-system-design.pdf",
                    Category = catDistributed,
                    ViewsCount = 980,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4)
                },
                new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "توسعه نرم‌افزار مبتنی بر آزمون (TDD) در سی‌شارپ",
                    Slug = "practical-tdd-csharp",
                    Subtitle = "روش‌شناسی تست خودکار و تضمین کیفیت نرم‌افزار در پروژه‌های واقعی",
                    Description = "راهنمای عملی و گام‌به‌گام پیاده‌سازی آزمون‌های واحد، آزمون‌های یکپارچه‌سازی، ایزولاسیون با Mocking و یکپارچه‌سازی در خطوط لوله CI/CD.",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?auto=format&fit=crop&w=800&q=80",
                    Publisher = "انتشارات مهندسی نرم‌افزار",
                    PublishYear = "۱۴۰۱",
                    PagesCount = 312,
                    ISBN = "978-600-12345-3-3",
                    ExternalBuyLink = "https://example.com/buy-tdd-csharp",
                    DigitalDownloadLink = "https://example.com/sample-tdd.pdf",
                    Category = catArch,
                    ViewsCount = 730,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2)
                }
            };

            await _context.Books.AddRangeAsync(books);
            await _context.SaveChangesAsync();
            seededBookCount = books.Count;
            _logger.LogInformation("Successfully seeded {Count} books into SQL Server.", seededBookCount);
        }
        else
        {
            var existingCleanArchBook = await _context.Books.FirstOrDefaultAsync(b => b.Slug == "hands-on-clean-architecture-dotnet");
            if (existingCleanArchBook != null && existingCleanArchBook.ThumbnailUrl != "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=800&q=80")
            {
                existingCleanArchBook.ThumbnailUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=800&q=80";
                await _context.SaveChangesAsync();
            }
        }

        // 5. Seed Courses if empty (At least 3 courses with Aparat videos)
        if (!await _context.Courses.AnyAsync())
        {
            _logger.LogInformation("Seeding initial video training courses into SQL Server...");

            var courses = new List<Course>
            {
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "دوره جامع معماری تمیز و طراحی دامنه‌محور (DDD) در دات‌نت ۹",
                    Slug = "mastering-clean-architecture-dotnet-9",
                    Description = "آموزش پیشرفته پیاده‌سازی عملی معماری تمیز، الگوهای DDD، تفکیک لایه‌ها، مدیریت ترانزاکشن‌ها، تست‌نویسی خودکار و ساخت وب‌سرویس‌های سازمانی با بالاترین استاندارد مهندسی.",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&w=1200&q=80",
                    AparatVideoUrl = "https://www.aparat.com/v/sample123",
                    Level = "پیشرفته",
                    Duration = "۳۲ ساعت",
                    LessonsCount = 48,
                    OriginalPrice = 4_800_000,
                    DiscountedPrice = 3_360_000,
                    DiscountPercentage = 30,
                    Category = catArch,
                    ViewsCount = 3200,
                    CreatedAt = DateTime.UtcNow.AddMonths(-5)
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "طراحی میکروسرویس‌های رویدادمحور با RabbitMQ و Docker",
                    Slug = "microservices-event-driven-architecture",
                    Description = "دوره جامع و کاربردی طراحی میکروسرویس‌های مقیاس‌پذیر در دات‌نت، ارتباطات ناهمگام، صف‌بندی پیام با RabbitMQ، الگوی Saga، گیت‌وی API و استقرار در کانتینرهای داکر.",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?auto=format&fit=crop&w=1200&q=80",
                    AparatVideoUrl = "https://www.aparat.com/v/sample456",
                    Level = "جامع",
                    Duration = "۴۵ ساعت",
                    LessonsCount = 64,
                    OriginalPrice = 6_500_000,
                    DiscountedPrice = 4_875_000,
                    DiscountPercentage = 25,
                    Category = catDistributed,
                    ViewsCount = 4150,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                },
                new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "بهینه‌سازی کارایی و ترفندهای پیشرفته دات‌نت و C#",
                    Slug = "advanced-csharp-performance-tuning",
                    Description = "نکات عمیق مدیریت حافظه با Span و Memory، بهینه‌سازی Garbage Collection، الگوهای Asynchronous بدون بن‌بست و دستیابی به حداکثر سرعت در سرویس‌های پردازش سنگین.",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=1200&q=80",
                    AparatVideoUrl = "https://www.aparat.com/v/sample789",
                    Level = "متوسط",
                    Duration = "۱۸ ساعت",
                    LessonsCount = 26,
                    OriginalPrice = 2_800_000,
                    DiscountedPrice = 2_240_000,
                    DiscountPercentage = 20,
                    Category = catArch,
                    ViewsCount = 1980,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                }
            };

            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();
            seededCourseCount = courses.Count;
            _logger.LogInformation("Successfully seeded {Count} courses into SQL Server.", seededCourseCount);
        }

        return (seededCatCount, seededArticleCount, seededBookCount, seededCourseCount);
    }
}
