namespace ErpSystem.SharedKernel.Pagination;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedResult<T> Empty(int pageSize) => new(Array.Empty<T>(), 1, pageSize, 0);
}

public record PaginationParams(int PageNumber = 1, int PageSize = 10)
{
    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;

    public static PaginationParams Default => new();
}
