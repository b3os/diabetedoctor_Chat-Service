namespace ChatService.Contract.Services.Media.Responses;

public record UploadMediaResponse
{
    public List<string> MediaIds { get; init; } = [];
}