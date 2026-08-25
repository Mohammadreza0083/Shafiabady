using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.Services;

/// <summary>
/// Service implementation for recording entity views and tracking engagement metrics.
/// </summary>
public class ViewTrackerService : IViewTrackerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ViewTrackerService> _logger;

    public ViewTrackerService(IUnitOfWork unitOfWork, ILogger<ViewTrackerService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> RecordViewAsync(EntityType entityType, Guid entityId, string ipAddress, string? userAgent = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ipAddress = "127.0.0.1";
        }

        var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);

        // Check if this IP viewed this entity within the past 24 hours
        var recentLogs = await _unitOfWork.Repository<ViewLog>().FindAsync(v =>
            v.EntityType == entityType &&
            v.EntityId == entityId &&
            v.IpAddress == ipAddress &&
            v.ViewedAt >= twentyFourHoursAgo, ct);

        if (recentLogs.Any())
        {
            _logger.LogWarning("Duplicate view attempt within 24 hours for {EntityType} {EntityId} from IP {IpAddress}. View count not incremented.",
                entityType, entityId, ipAddress);
            return Result<bool>.Success(false);
        }

        // Record new ViewLog entry
        var viewLog = new ViewLog
        {
            EntityType = entityType,
            EntityId = entityId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ViewedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ViewLog>().AddAsync(viewLog, ct);

        // Increment the target entity's view counter
        switch (entityType)
        {
            case EntityType.Article:
                var article = await _unitOfWork.Repository<Article>().GetByIdAsync(entityId, ct);
                if (article != null && !article.IsDeleted)
                {
                    article.ViewsCount++;
                    _unitOfWork.Repository<Article>().Update(article);
                }
                break;

            case EntityType.Book:
                var book = await _unitOfWork.Repository<Book>().GetByIdAsync(entityId, ct);
                if (book != null && !book.IsDeleted)
                {
                    book.ViewsCount++;
                    _unitOfWork.Repository<Book>().Update(book);
                }
                break;

            case EntityType.Course:
                var course = await _unitOfWork.Repository<Course>().GetByIdAsync(entityId, ct);
                if (course != null && !course.IsDeleted)
                {
                    course.ViewsCount++;
                    _unitOfWork.Repository<Course>().Update(course);
                }
                break;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Recorded new unique view for {EntityType} {EntityId} from IP {IpAddress}", entityType, entityId, ipAddress);
        return Result<bool>.Success(true);
    }

    public async Task<Result<ViewStatsDto>> GetViewStatsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Calculating platform view analytics and top content");

        var articles = await _unitOfWork.Repository<Article>().FindAsync(a => !a.IsDeleted, ct);
        var books = await _unitOfWork.Repository<Book>().FindAsync(b => !b.IsDeleted, ct);
        var courses = await _unitOfWork.Repository<Course>().FindAsync(c => !c.IsDeleted, ct);
        var pendingCommentsCount = await _unitOfWork.Repository<Comment>().CountAsync(c => !c.IsDeleted && c.Status == CommentStatus.Pending, ct);

        var totalArticleViews = articles.Sum(a => a.ViewsCount);
        var totalBookViews = books.Sum(b => b.ViewsCount);
        var totalCourseViews = courses.Sum(c => c.ViewsCount);

        var topArticle = articles.OrderByDescending(a => a.ViewsCount).FirstOrDefault();
        var topBook = books.OrderByDescending(b => b.ViewsCount).FirstOrDefault();
        var topCourse = courses.OrderByDescending(c => c.ViewsCount).FirstOrDefault();

        var stats = new ViewStatsDto
        {
            TotalViews = totalArticleViews + totalBookViews + totalCourseViews,
            TotalArticlesViews = totalArticleViews,
            TotalBooksViews = totalBookViews,
            TotalCoursesViews = totalCourseViews,
            TopArticleTitle = topArticle?.Title ?? string.Empty,
            TopArticleViews = topArticle?.ViewsCount ?? 0,
            TopBookTitle = topBook?.Title ?? string.Empty,
            TopBookViews = topBook?.ViewsCount ?? 0,
            TopCourseTitle = topCourse?.Title ?? string.Empty,
            TopCourseViews = topCourse?.ViewsCount ?? 0,
            PendingCommentsCount = pendingCommentsCount
        };

        return Result<ViewStatsDto>.Success(stats);
    }
}
