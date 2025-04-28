namespace ChatService.Contract.Abstractions.Shared;

public class PagedList<T> 
{
    public List<T> Items { get; set; }
    public int TotalItems { get; init; }
    public int PageSize { get; init; }
    public string NextCursor { get; init; }
    public bool HasNext { get; init; }

    private PagedList(List<T> items, int total, int pageSize, string nextCursor)
    {
        Items = items;
        TotalItems = total;
        PageSize = pageSize;
        NextCursor = nextCursor;
        HasNext = total < 10;
        // AddRange(items);
    }
    
    public static PagedList<T> Create(List<T> items, int total, int pageSize, string nextCursor)
        => new(items, total, pageSize, nextCursor);
    
}