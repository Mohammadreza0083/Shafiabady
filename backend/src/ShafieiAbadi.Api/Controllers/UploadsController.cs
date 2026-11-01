using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Endpoint for uploading images (cover images, thumbnails, and inline rich-text content images) from the admin panel.
/// </summary>
public class UploadsController : BaseApiController
{
    /// <summary>Maximum accepted image size (5 MB).</summary>
    public const long MaxImageBytes = 5 * 1024 * 1024;

    /// <summary>Public URL prefix under which uploaded files are served as static content.</summary>
    public const string PublicPathPrefix = "/uploads";

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadsController> _logger;

    public UploadsController(IWebHostEnvironment environment, ILogger<UploadsController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a single image file (JPEG, PNG, WebP or GIF, up to 5 MB) and returns its public URL (Admin only).
    /// </summary>
    /// <param name="file">Multipart form file field named "file".</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Absolute public URL of the stored image.</returns>
    [HttpPost("image")]
    [Authorize]
    [RequestSizeLimit(MaxImageBytes + 64 * 1024)]
    [ProducesResponseType(typeof(Result<UploadedImageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<UploadedImageDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<UploadedImageDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return HandleResult(Result<UploadedImageDto>.Failure("فایلی برای آپلود ارسال نشده است."));

        if (file.Length > MaxImageBytes)
            return HandleResult(Result<UploadedImageDto>.Failure("حجم تصویر نباید بیشتر از ۵ مگابایت باشد."));

        // Detect the real format from magic bytes instead of trusting the client-supplied content type or extension
        var header = new byte[12];
        await using (var probe = file.OpenReadStream())
        {
            _ = await probe.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        }

        var extension = DetectImageExtension(header);
        if (extension == null)
            return HandleResult(Result<UploadedImageDto>.Failure("فقط تصاویر JPEG، PNG، WebP و GIF مجاز هستند."));

        var subFolder = DateTime.UtcNow.ToString("yyyy/MM");
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var targetDir = Path.Combine(webRoot, "uploads", subFolder);
        Directory.CreateDirectory(targetDir);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using (var target = System.IO.File.Create(Path.Combine(targetDir, fileName)))
        {
            await file.CopyToAsync(target, ct);
        }

        var url = $"{Request.Scheme}://{Request.Host}{PublicPathPrefix}/{subFolder}/{fileName}";
        _logger.LogInformation("Stored uploaded image {FileName} ({Bytes} bytes).", fileName, file.Length);

        return HandleResult(Result<UploadedImageDto>.Success(new UploadedImageDto(url), StatusCodes.Status201Created));
    }

    private static string? DetectImageExtension(byte[] h)
    {
        if (h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ".jpg";
        if (h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return ".png";
        if (h[0] == 0x47 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x38) return ".gif";
        if (h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 &&
            h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50) return ".webp";
        return null;
    }
}

/// <summary>
/// Response payload for an uploaded image.
/// </summary>
/// <param name="Url">Absolute public URL of the stored image.</param>
public record UploadedImageDto(string Url);
