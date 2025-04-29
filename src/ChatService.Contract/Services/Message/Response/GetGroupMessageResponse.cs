using ChatService.Contract.DTOs.MessageDtos;

namespace ChatService.Contract.Services.Message.Response;

public record GetGroupMessageResponse
{
    public PagedList<MessageDto> Messages { get; init; } = default!;
}