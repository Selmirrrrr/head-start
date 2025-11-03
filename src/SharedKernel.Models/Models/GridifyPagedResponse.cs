namespace HeadStart.SharedKernel.Models.Models;

/// <summary>
/// Generic paged response for data grids with filtering, sorting, and pagination
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public record GridifyPagedResponse<T>
{
    /// <summary>
    /// The data items for the current page
    /// </summary>
    public required IReadOnlyList<T> Data { get; init; }

    /// <summary>
    /// Total number of items matching the filter
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Current page number (0-based)
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages - 1;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 0;
}
