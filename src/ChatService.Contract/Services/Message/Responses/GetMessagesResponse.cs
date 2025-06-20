using ChatService.Contract.DTOs.MessageDtos;

namespace ChatService.Contract.Services.Message.Responses;

public record GetMessagesResponse
{
    public PagedList<MessageDto> Messages { get; init; } = null!;
}