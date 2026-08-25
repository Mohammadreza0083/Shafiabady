using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.Services;

public class ViewTrackerService : IViewTrackerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ViewTrackerService> _logger;

    public ViewTrackerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ViewTrackerService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<bool>> RecordViewAsync(EntityType entityType, Guid entityId, string ipAddress, string? userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            ipAddress = "127.0.0.1";

        var cutoffTime = DateTime.UtcNow.AddHours(-24);

        // Check if a view from this IP for this entity occurred in the last 24 hours
        var existingLogs = await _unitOfWork.Repository<ViewLog>()
            .FindAsync(v => v.EntityType == entityType && v.EntityId == entityId && v.IpAddress == ipAddress && v.ViewedAt >= cutoffTime, ct);

        if (existingLogs.Any())
        {
            _logger.LogWarning("Duplicate view attempt for {EntityType} {EntityId} from IP {IpAddress} within 24-hour window", entityType, entityId, ipAddress);
            return Result<bool>.Success(false);
        }

        // Add new ViewLog
        var viewLog = new ViewLog
        {
            EntityType = entityType,
            EntityId = entityId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ViewedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<ViewLog>().AddAsync(viewLog, ct);

        // Increment entity views count
        switch (entityType)
        {
            case EntityType.Article:
                var article = await _unitOfWork.Repository<Article>().GetByIdAsync(entityId, ct);
                if (article != null)
                {
                    article.ViewsCount++;
                    _unitOfWork.Repository<Article>().Update(article);
                }
                break;

            case EntityType.Book:
                var book = await _unitOfWork.Repository<Book>().GetByIdAsync(entityId, ct);
                if (book != null)
                {
                    book.ViewsCount++;
                    _unitOfWork.Repository<Book>().Update(book);
                }
                break;

            case EntityType.Course:
                var course = await _unitOfWork.Repository<Course>().GetByIdAsync(entityId, ct);
                if (course != null)
                {
                    course.ViewsCount++;
                    _unitOfWork.Repository<Course>().Update(course);
                }
                break;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("View successfully registered for {EntityType} {EntityId} from IP {IpAddress}", entityType, entityId, ipAddress);

        return Result<bool>.Success(true);
    }

    public async Task<Result<ViewStatsDto>> GetViewStatsAsync(CancellationToken ct = default)
    {
        var totalViews = await _unitOfWork.Repository<ViewLog>().CountAsync(null, ct);

        var allArticles = await _unitOfWork.Repository<Article>().FindAsync(a => !a.IsDeleted && a.IsPublished, ct);
        var topArticle = allArticles.OrderByDescending(a => a.ViewsCount).FirstOrDefault();

        var allBooks = await _unitOfWork.Repository<Book>().FindAsync(b => !b.IsDeleted, ct);
        var topBook = allBooks.OrderByDescending(b => b.ViewsCount).FirstOrDefault();

        var allCourses = await _unitOfWork.Repository<Course>().FindAsync(c => !c.IsDeleted, ct);
        var topCourse = allCourses.OrderByDescending(c => c.ViewsCount).FirstOrDefault();

        var pendingComments = await _unitOfWork.Repository<Comment>()
            .CountAsync(c => !c.IsDeleted && c.Status == CommentStatus.Pending, ct);

        var stats = new ViewStatsDto
        {
            TotalViews = totalViews,
            TopArticle = topArticle != null ? _mapper.Map<ArticleDto>(topArticle) : null,
            TopBook = topBook != null ? _mapper.Map<BookDto>(topBook) : null,
            TopCourse = topCourse != null ? _mapper.Map<CourseDto>(topCourse) : null,
            PendingCommentsCount = pendingComments
        };

        return Result<ViewStatsDto>.Success(stats);
    }
}
