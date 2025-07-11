using ChatService.Contract.DTOs.ValueObjectDtos;

namespace ChatService.Contract.Services.Conversation.Commands.IntegrationCommand;

public record UpdateLastMessageInConversationCommand : ICommand
{
    public ObjectId ConversationId { get; init; }
    public string? SenderId { get; init; }
    public ObjectId MessageId { get; init; }
    public string? MessageContent { get; init; }
    public int MessageType { get; init; }
    public FileAttachmentDto? FileAttachmentDto { get; init; }
    public DateTime? CreatedDate { get; init; }
}