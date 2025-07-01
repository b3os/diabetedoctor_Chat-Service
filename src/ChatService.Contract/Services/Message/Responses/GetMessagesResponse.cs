using ChatService.Contract.DTOs.MessageDtos;

namespace ChatService.Contract.Services.Message.Responses;

public record GetMessagesResponse
{
    public PagedList<MessageResponseDto> Messages { get; init; } = null!;
}