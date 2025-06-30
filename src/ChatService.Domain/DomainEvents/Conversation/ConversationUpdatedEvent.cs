namespace ChatService.Domain.DomainEvents.Conversation;

public record ConversationUpdatedEvent(string ConversationId, string? Name, string? OldAvatar) : IDomainEvent;