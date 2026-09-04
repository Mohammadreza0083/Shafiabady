using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Domain.Enums;
using ShafieiAbadi.Infrastructure.Data;

namespace ShafieiAbadi.Infrastructure.BackgroundServices;

/// <summary>
/// Background worker recurring every 24 hours to permanently purge rejected comments older than 7 days.
/// </summary>
public class RejectedCommentsPurgeBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RejectedCommentsPurgeBackgroundService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public RejectedCommentsPurgeBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RejectedCommentsPurgeBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RejectedCommentsPurgeBackgroundService started. Purge interval: {Interval}", Interval);

        // Allow application startup to stabilize before the initial purge run
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeExpiredRejectedCommentsAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error occurred during execution of rejected comments purge cycle.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("RejectedCommentsPurgeBackgroundService stopped.");
    }

    private async Task PurgeExpiredRejectedCommentsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cutoffDate = DateTime.UtcNow.AddDays(-7);

        // Find all rejected comments whose RejectedAt timestamp is older than 7 days
        var expiredComments = await context.Comments
            .Where(c => c.Status == CommentStatus.Rejected && c.RejectedAt.HasValue && c.RejectedAt.Value <= cutoffDate)
            .ToListAsync(ct);

        if (expiredComments.Count > 0)
        {
            _logger.LogInformation("Found {Count} rejected comments older than 7 days (cutoff: {CutoffDate}). Purging permanently...",
                expiredComments.Count, cutoffDate);

            context.Comments.RemoveRange(expiredComments);
            await context.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully purged {Count} rejected comments permanently from database.", expiredComments.Count);
        }
        else
        {
            _logger.LogDebug("No rejected comments older than 7 days found during purge cycle.");
        }
    }
}
