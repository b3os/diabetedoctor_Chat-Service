namespace ChatService.Contract.Common.Filters;

public class QueryOffsetFilter
{
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SortType { get; set; }
    public bool? IsSortDesc { get; set; }
    public string? Search { get; set; } = null;
}