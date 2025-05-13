using ChatService.Contract.DTOs.EnumDtos;
using ChatService.Contract.DTOs.MessageDtos;

namespace ChatService.Contract.Services.Message.Commands;

public record CreateMessageCommand : ICommand
{
    public ObjectId GroupId { get; init; }
    public string UserId { get; init; } = default!;
    public string? Content {get; init;}
    public MessageTypeDto Type {get; init;}
}