namespace ChatService.Contract.Common.Filters;

public class QueryCursorFilter
{
    public string? Cursor { get; set; } = null;
    public int? PageSize { get; set; } = null;
    public string? Sort { get; set; } = null;
    public string? Direction { get; set; } = null;
    public string? Search { get; set; } = null;
}