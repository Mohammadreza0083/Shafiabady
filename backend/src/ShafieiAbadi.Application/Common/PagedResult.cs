namespace ShafieiAbadi.Application.Common;

/// <summary>
/// Encapsulates a paginated list of items alongside pagination metadata for infinite scrolling and grid views.
/// </summary>
/// <typeparam name="T">Type of items in the page.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Collection of items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Total count of items matching the query criteria across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Current 1-based page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Page size (number of items requested per page).
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Calculated total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates whether there is a subsequent page available.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Indicates whether there is a preceding page available.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Initializes a new instance of the PagedResult class.
    /// </summary>
    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize > 0 ? pageSize : 10;
    }
}
