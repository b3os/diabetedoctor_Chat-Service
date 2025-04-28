namespace ChatService.Contract.Services;

public class QueryFilter
{
    public string? Cursor { get; set; } = default;
    public int? PageSize { get; set; } = default;
    public string? Sort { get; set; } = default;
    public string? Direction { get; set; } = default;
    public string? Search { get; set; } = default;
}