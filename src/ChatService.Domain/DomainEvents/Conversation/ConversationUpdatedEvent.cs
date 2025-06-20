namespace ChatService.Domain.DomainEvents.Conversation;

public record ConversationUpdatedEvent(string GroupId, string? Name, string? Avatar) : IDomainEvent;