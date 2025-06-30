using ChatService.Contract.Services.Media.Responses;

namespace ChatService.Contract.Services.Media.Commands;

public record UploadMediaCommand : ICommand<Response<UploadMediaResponse>>
{
    public List<IFormFile> Files { get; init; } = [];
    public string? UploaderId { get; init; }
};