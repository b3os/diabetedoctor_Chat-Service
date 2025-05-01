using ChatService.Contract.DTOs.MessageDtos;

namespace ChatService.Contract.Services.Message.Commands;

public record CreateMessageCommand : ICommand
{
    public string GroupId { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public MessageCreateDto Message { get; init; } = default!;
}