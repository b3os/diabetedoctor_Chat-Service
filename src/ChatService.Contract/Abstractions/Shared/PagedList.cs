namespace ChatService.Contract.Abstractions.Shared;

public class PagedList<T> 
{
    public List<T> Items { get; set; }
    public int TotalItems { get; init; }
    public int PageSize { get; init; }
    public string NextCursor { get; init; }
    public bool HasNextPage { get; init; }

    private PagedList(List<T> items, int total, int pageSize, string nextCursor, bool hasNextPage)
    {
        Items = items;
        TotalItems = total;
        PageSize = pageSize;
        NextCursor = nextCursor;
        HasNextPage = hasNextPage;
    }
    
    public static PagedList<T> Create(List<T> items, int total, int pageSize, string nextCursor, bool hasNext)
        => new(items, total, pageSize, nextCursor, hasNext);
    
}