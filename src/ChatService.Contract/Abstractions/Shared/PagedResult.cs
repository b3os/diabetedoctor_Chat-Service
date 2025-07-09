namespace ChatService.Contract.Abstractions.Shared;

public class PagedResult<T>
{
    private PagedResult(List<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Items = items;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
    public List<T> Items { get; }
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public bool HasNextPage => PageIndex * PageSize < TotalCount;
    public bool HasPreviousPage => PageIndex > 1;
    
    public static PagedResult<T> Create(List<T> items, int pageIndex, int pageSize, int totalCount)
        => new(items, pageIndex, pageSize, totalCount);
}
