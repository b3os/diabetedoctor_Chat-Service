using ChatService.Contract.DTOs;

namespace ChatService.Contract.EventBus.Events.MessageIntegrationEvents;

public record MessageCreatedIntegrationEvent : IntegrationEvent
{
    public SenderInfo? Sender { get; init; }
    public ConversationInfo? Conversation { get; init; }
    public string? MessageId { get; init; }
    public string? MessageContent { get; init; }
    public int MessageType { get; init; }
    public FileAttachmentDto? FileAttachment { get; init; }
    public DateTime? CreatedDate { get; init; }
}

public record SenderInfo
{
    public string? SenderId { get; init; }
    public string? FullName { get; init; }
    public string? Avatar { get; init; }
}

public record ConversationInfo
{
    public string? ConversationId { get; init; }
    public string? ConversationName { get; init; }
    public string? Avatar { get; init; }
    public int ConversationType {get; init; }
}