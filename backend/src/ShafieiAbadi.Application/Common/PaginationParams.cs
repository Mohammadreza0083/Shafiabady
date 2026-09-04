namespace ShafieiAbadi.Application.Common;

/// <summary>
/// Query parameters for filtering, searching, and paginating entity collections.
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    /// <summary>
    /// Requested page number (1-based index, defaults to 1).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Requested items count per page (defaults to 10, capped at 50).
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }

    /// <summary>
    /// Optional search term matching against titles, summaries, or content.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Alias for Search parameter (matching frontend query naming conventions).
    /// </summary>
    public string? SearchTerm
    {
        get => Search;
        set => Search = value;
    }

    /// <summary>
    /// Optional category slug filter.
    /// </summary>
    public string? CategorySlug { get; set; }

    /// <summary>
    /// Alias for CategorySlug parameter (matching frontend query naming conventions).
    /// </summary>
    public string? Category
    {
        get => CategorySlug;
        set => CategorySlug = value;
    }

    /// <summary>
    /// Optional category unique identifier filter.
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Optional tag keyword filter.
    /// </summary>
    public string? Tag { get; set; }
}
