using ChatService.Contract.Services.Media.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Contract.Services.Media.Commands;

public record UploadMediaCommand : ICommand<Response<UploadMediaResponse>>
{
    public required IFormFileCollection Files { get; set; }
    public string? UploaderId { get; init; }
};