import type { Article, Book, Course, Comment } from '../types';

export const mockArticles: Article[] = [
  {
    id: 'a101b001-1111-4444-8888-100000000001',
    slug: 'clean-architecture-dotnet-9',
    title: 'راهنمای جامع پیاده‌سازی معماری تمیز در دات‌نت ۹',
    summary: 'بررسی ساختار لایه‌بندی Enterprise، تفکیک Domain و Application، الگوهای CQRS و استراتژی‌های تست‌پذیری در مقیاس بالا.',
    content: `
      <h2>مقدمه بر Clean Architecture در اکوسیستم دات‌نت</h2>
      <p>معماری تمیز (Clean Architecture) که توسط رابرت سی. مارتین (Uncle Bob) معرفی شد، بر اصل استقلال منطق تجاری (Business Logic) از فریم‌ورک‌ها، دیتابیس‌ها و رابط‌های کاربری استوار است.</p>
      
      <h3>لایه‌بندی استاندارد در .NET</h3>
      <p>در این ساختار، لایه‌ها به صورت زیر سازمان‌دهی می‌شوند:</p>
      <ul>
        <li><strong>Domain Layer:</strong> شامل Entities، Value Objects، Enums و Domain Events. این لایه هیچ وابستگی خارجی ندارد.</li>
        <li><strong>Application Layer:</strong> شامل Use Cases، Commands، Queries و Interfaces.</li>
        <li><strong>Infrastructure Layer:</strong> پیاده‌سازی دسترسی به داده (EF Core)، سرویس‌های ایمیل، کش Redis و پیام‌رسان‌ها.</li>
        <li><strong>Presentation / API Layer:</strong> کنترلرها، Minimal APIs و Middlewares.</li>
      </ul>

      <h3>چرا دات‌نت ۹ بهترین انتخاب برای سیستم‌های مقیاس‌پذیر است؟</h3>
      <p>با معرفی بهینه‌سازی‌های عمیق در JIT Compiler و Garbage Collector در دات‌نت ۹، عملکرد سیستم‌های مبتنی بر معماری تمیز به بالاترین حد خود رسیده است.</p>
    `,
    coverImage: '',
    readTime: '۸ دقیقه',
    category: 'معماری نرم‌افزار',
    tags: ['دات‌نت', 'Clean Architecture', 'سیستم‌های توزیع‌شده', 'C#'],
    viewsCount: 1420,
    commentsCount: 12,
    createdAt: '2026-08-20T10:30:00Z',
    author: {
      name: 'دکتر محمد حسین شفیع‌آبادی',
      avatar: '',
      role: 'دانشیار و پژوهشگر مهندسی نرم‌افزار',
    },
  },
  {
    id: 'a101b002-2222-4444-8888-200000000002',
    slug: 'domain-driven-design-in-practice',
    title: 'اصول Domain-Driven Design (DDD) در پروژه‌های واقعی',
    summary: 'چگونه از الگوهای استراتژیک و تاکتیکی DDD برای مدل‌سازی دامنه‌های پیچیده تجاری و مدیریت رخدادهای دامنه استفاده کنیم؟',
    content: `
      <h2>چالش مدل‌سازی دامنه تجاری</h2>
      <p>طراحی دامنه‌محور (DDD) فراتر از کدنویسی است؛ این رویکرد یک زبان مشترک (Ubiquitous Language) میان مهندسان نرم‌افزار و کارشناسان دامنه خلق می‌کند.</p>
      
      <h3>Aggregates و Aggregate Roots</h3>
      <p>یکپارچگی و مرزهای تراکنشال در قلب Aggregateها نهفته است. هر تغییر داده‌ای باید از دروازه Aggregate Root عبور کند تا قواعد تجاری (Invariants) نقض نشوند.</p>
    `,
    coverImage: '',
    readTime: '۱۲ دقیقه',
    category: 'طراحی سیستم',
    tags: ['DDD', 'مدل‌سازی دامنه', 'Architecture'],
    viewsCount: 980,
    commentsCount: 8,
    createdAt: '2026-08-18T14:15:00Z',
    author: {
      name: 'دکتر محمد حسین شفیع‌آبادی',
      avatar: '',
      role: 'دانشیار و پژوهشگر مهندسی نرم‌افزار',
    },
  },
  {
    id: 'a101b003-3333-4444-8888-300000000003',
    slug: 'test-driven-development-tdd-guide',
    title: 'توسعه آزمون‌محور (TDD) با xUnit، Moq و FluentAssertions',
    summary: 'پیاده‌سازی چرخه Red-Green-Refactor، نگارش آزمون‌های واحد تمیز، شبیه‌سازی وابستگی‌ها و اطمینان از پوشش حداکثری کد.',
    content: `
      <h2>فلسفه TDD در تیم‌های پیشرو</h2>
      <p>در TDD، تست‌ها صرفاً ابزار اعتبارسنجی نیستند، بلکه ابزار طراحی نرم‌افزار هستند. با نوشتن تست پیش از کد اصلی، شما خود را ملزم به طراحی اینترفیس‌های ماژولار و تست‌پذیر می‌کنید.</p>
    `,
    coverImage: '',
    readTime: '۱۰ دقیقه',
    category: 'کیفیت و تست',
    tags: ['TDD', 'xUnit', 'Moq', 'تست نرم‌افزار'],
    viewsCount: 750,
    commentsCount: 5,
    createdAt: '2026-08-15T09:00:00Z',
    author: {
      name: 'دکتر محمد حسین شفیع‌آبادی',
      avatar: '',
      role: 'دانشیار و پژوهشگر مهندسی نرم‌افزار',
    },
  },
  {
    id: 'a101b004-4444-4444-8888-400000000004',
    slug: 'microservices-cqrs-event-sourcing',
    title: 'الگوهای CQRS و Event Sourcing در میکروسرویس‌ها',
    summary: 'تفکیک مدل‌های خواندن و نوشتن، مدیریت تاریخچه رخدادها با Kafka و ساخت نماهای داده بهینه برای بارهای کاری بالا.',
    content: `
      <h2>چرا CRUD در مقیاس بالا پاسخگو نیست؟</h2>
      <p>وقتی نسبت خواندن به نوشتن ۱۰۰ به ۱ می‌شود، ترکیب مدل‌های داده به گلوگاه تبدیل می‌شود. CQRS به شما اجازه می‌دهد مدل نوشتن را برای اعتبارسنجی و مدل خواندن را برای سرعت بی‌نظیر بهینه کنید.</p>
    `,
    coverImage: '',
    readTime: '۱۵ دقیقه',
    category: 'میکروسرویس',
    tags: ['CQRS', 'Event Sourcing', 'Kafka', 'Microservices'],
    viewsCount: 1650,
    commentsCount: 18,
    createdAt: '2026-08-10T16:45:00Z',
    author: {
      name: 'دکتر محمد حسین شفیع‌آبادی',
      avatar: '',
      role: 'دانشیار و پژوهشگر مهندسی نرم‌افزار',
    },
  },
  {
    id: 'a101b005-5555-4444-8888-500000000005',
    slug: 'astro-js-hybrid-rendering-guide',
    title: 'بررسی عمیق معماری هیبریدی در Astro.js',
    summary: 'ترکیب سرعت رندر ایستا (SSG) و انعطاف‌پذیری رندر سمت سرور (SSR) در ساخت پلتفرم‌های محتوا‌محور فوق سریع.',
    content: `
      <h2>تحول توسعه وب با Astro Islands</h2>
      <p>فریم‌ورک Astro با معماری جزیره‌ای (Islands Architecture)، ارسال جاوااسکریپت به مرورگر را به حداقل می‌رساند و امتیاز ۱۰۰ در ابزارهای Core Web Vitals را فراهم می‌آورد.</p>
    `,
    coverImage: '',
    readTime: '۷ دقیقه',
    category: 'فرانت‌اند مدرن',
    tags: ['Astro', 'TailwindCSS', 'SSR', 'Web Performance'],
    viewsCount: 1120,
    commentsCount: 9,
    createdAt: '2026-08-05T11:20:00Z',
    author: {
      name: 'دکتر محمد حسین شفیع‌آبادی',
      avatar: '',
      role: 'دانشیار و پژوهشگر مهندسی نرم‌افزار',
    },
  },
  {
    id: 'a101b006-6666-4444-8888-600000000006',
    slug: 'high-performance-ef-core-optimizations',
    title: 'تکنیک‌های بهینه‌سازی پیشرفته در EF Core',
    summary: 'جلوگیری از N+1 queries، استفاده از AsNoTracking، کامپایل کردن کوئری‌ها و بهینه‌سازی ایندکس‌ها در SQL Server.',
    content: `
      <h2>بیشینه‌سازی کارایی پایگاه‌داده</h2>
      <p>Entity Framework Core یکی از قدرتمندترین ORMهای دنیای نرم‌افزار است، اما استفاده نادرست از آن می‌تواند به فاجعه عملکردی ختم شود.</p>
    `,
    readTime: '۹ دقیقه',
    category: 'پایگاه داده',
    tags: ['EF Core', 'SQL Server', 'Performance', 'دات‌نت'],
    viewsCount: 1890,
    commentsCount: 14,
    createdAt: '2026-07-28T08:10:00Z',
    coverImage: '',
    author: {
      name: 'دکتر محمد حسین شفیع‌آبادی',
      avatar: '',
      role: 'دانشیار و پژوهشگر مهندسی نرم‌افزار',
    },
  },
];

export const mockBooks: Book[] = [
  {
    id: 'b101b001-1111-4444-8888-100000000001',
    slug: 'hands-on-clean-architecture-dotnet',
    title: 'مرجع جامع Clean Architecture در .NET',
    subtitle: 'راهنمای عملی و صنعتی ساخت نرم‌افزارهای سازمانی مقیاس‌پذیر',
    description: 'کتابی مرجع و کاربردی شامل الگوهای طراحی DDD، پیاده‌سازی CQRS با MediatR، مدیریت Transactionها و راهکارهای تست‌نویسی مدرن.',
    coverImage: '',
    pagesCount: 480,
    publisher: 'انتشارات تخصصی مهندسی نرم‌افزار',
    publishYear: '۱۴۰۴',
    viewsCount: 3200,
    createdAt: '2026-01-15T00:00:00Z',
  },
  {
    id: 'b101b002-2222-4444-8888-200000000002',
    slug: 'mastering-system-design-persian',
    title: 'اصول طراحی سیستم‌های پرترافیک',
    subtitle: 'از مفاهیم پایه تا معماری‌های مقیاس‌بزرگ ابری',
    description: 'بررسی عمیق کشینگ توزیع‌شده، پارتیشن‌بندی دیتابیس، صف‌های پیام، مقاومت‌پذیری در برابر خطا و تکنیک‌های توزیع بار.',
    coverImage: '',
    pagesCount: 520,
    publisher: 'انتشارات دانش و فناوری',
    publishYear: '۱۴۰۳',
    viewsCount: 2850,
    createdAt: '2025-11-20T00:00:00Z',
  },
  {
    id: 'b101b003-3333-4444-8888-300000000003',
    slug: 'practical-tdd-csharp',
    title: 'توسعه آزمون‌محور کاربردی در سی‌شارپ',
    subtitle: 'هنر نوشتن کدهای تمیز، پایدار و قابل نگهداری',
    description: 'راهنمای گام‌به‌گام پیاده‌سازی تست‌های واحد، یکپارچگی و تست‌های رفتاری با ابزارهای xUnit و FluentAssertions.',
    coverImage: '',
    pagesCount: 360,
    publisher: 'انتشارات تخصصی مهندسی نرم‌افزار',
    publishYear: '۱۴۰۳',
    viewsCount: 1940,
    createdAt: '2025-08-10T00:00:00Z',
  },
];

export const mockCourses: Course[] = [
  {
    id: 'c101b001-1111-4444-8888-100000000001',
    slug: 'mastering-clean-architecture-dotnet-9',
    title: 'دوره جامع مسترکلاس Clean Architecture در .NET 9',
    description: 'صفر تا صد معماری تمیز، الگوهای DDD، تست‌نویسی TDD، احراز هویت با JWT و استقرار در کانتینرهای Docker.',
    thumbnail: '',
    duration: '۴۲ ساعت',
    lessonsCount: 65,
    level: 'پیشرفته',
    price: 3500000,
    originalPrice: 4800000,
    discountedPrice: 3500000,
    discountPercentage: 27,
    isFree: false,
    viewsCount: 4500,
    createdAt: '2026-03-01T00:00:00Z',
  },
  {
    id: 'c101b002-2222-4444-8888-200000000002',
    slug: 'microservices-event-driven-architecture',
    title: 'طراحی و پیاده‌سازی سیستم‌های میکروسرویس رویدادمحور',
    description: 'استفاده از RabbitMQ و Kafka، پیاده‌سازی Outbox Pattern، توزیع تراکنش‌ها با Saga Pattern و نظارت با OpenTelemetry.',
    thumbnail: '',
    duration: '۳۶ ساعت',
    lessonsCount: 48,
    level: 'جامع',
    price: 4200000,
    originalPrice: 5600000,
    discountedPrice: 4200000,
    discountPercentage: 25,
    isFree: false,
    viewsCount: 3800,
    createdAt: '2026-02-15T00:00:00Z',
  },
  {
    id: 'c101b003-3333-4444-8888-300000000003',
    slug: 'advanced-csharp-performance-tuning',
    title: 'تیونینگ و ارتقای کارایی پیشرفته در C# و .NET',
    description: 'تخصیص حافظه، Span<T> و Memory<T>، بهینه‌سازی Garbage Collection و ابزارهای بنچ‌مارک با BenchmarkDotNet.',
    thumbnail: '',
    duration: '۱۸ ساعت',
    lessonsCount: 26,
    level: 'متوسط',
    price: 1800000,
    originalPrice: 2400000,
    discountedPrice: 1800000,
    discountPercentage: 25,
    isFree: false,
    viewsCount: 2100,
    createdAt: '2026-01-20T00:00:00Z',
  },
  {
    id: 'c101b004-4444-4444-8888-400000000004',
    slug: 'modern-web-development-astro-tailwind',
    title: 'توسعه وب مدرن و پرسرعت با Astro و TailwindCSS',
    description: 'ساخت وب‌سایت‌های فوق‌سریع با رندر هیبریدی، سئو پیشرفته و طراحی مینیمال مدرن با الهام از شرکت‌های پیشرو.',
    thumbnail: '',
    duration: '۱۲ ساعت',
    lessonsCount: 18,
    level: 'مقدماتی',
    price: 0,
    isFree: true,
    viewsCount: 5200,
    createdAt: '2026-01-05T00:00:00Z',
  },
];

export const mockCommentsTree: Comment[] = [
  {
    id: 'cm-001',
    authorName: 'سارا کاظمی',
    authorEmail: 'sara@example.com',
    content: 'مقاله فوق‌العاده کاربردی بود! به خصوص بخش تفکیک لایه‌ها و مدیریت Domain Events بسیار شفاف و کاربردی توضیح داده شده بود.',
    createdAt: '2026-08-21T14:30:00Z',
    parentId: null,
    replies: [
      {
        id: 'cm-002',
        authorName: 'دکتر محمد حسین شفیع‌آبادی',
        authorEmail: 'author@shafieiabadi.ir',
        content: 'سلام سارا جان، بسیار متشکرم از بازخوردت. در پروژه‌های بزرگ همین تفکیک مرزها تضمین‌کننده مقیاس‌پذیری و پایداری تیم است.',
        createdAt: '2026-08-21T15:10:00Z',
        parentId: 'cm-001',
        replies: [
          {
            id: 'cm-003',
            authorName: 'امیرحسین رضایی',
            authorEmail: 'amir@example.com',
            content: 'استاد آیا برای پیاده‌سازی Domain Events استفاده از MediatR INotification را پیشنهاد می‌دهید یا پیام‌رسان‌های داخلی دات‌نت ۹ مانند Channel؟',
            createdAt: '2026-08-21T16:00:00Z',
            parentId: 'cm-002',
            replies: [
              {
                id: 'cm-004',
                authorName: 'دکتر محمد حسین شفیع‌آبادی',
                authorEmail: 'author@shafieiabadi.ir',
                content: 'برای رویدادهای همگام داخل یک پردازش (In-Process)، MediatR عالی است؛ اما برای عملیات آسنکرون سنگین Channels یا Outbox Pattern همراه با RabbitMQ/Kafka بهترین عملکرد را دارد.',
                createdAt: '2026-08-21T16:45:00Z',
                parentId: 'cm-003',
                replies: [],
              },
            ],
          },
        ],
      },
      {
        id: 'cm-005',
        authorName: 'مهدی یوسفی',
        authorEmail: 'mehdi@example.com',
        content: 'تشکر بابت این مقاله ارزشمند.',
        createdAt: '2026-08-22T09:12:00Z',
        parentId: 'cm-001',
        replies: [],
      },
    ],
  },
  {
    id: 'cm-006',
    authorName: 'نیما اسدی',
    authorEmail: 'nima@example.com',
    content: 'منتظر انتشار دوره ویدیویی مرتبط با این موضوع هستیم. خسته نباشید!',
    createdAt: '2026-08-23T11:00:00Z',
    parentId: null,
    replies: [],
  },
];
