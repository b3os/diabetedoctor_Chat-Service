namespace ChatService.Domain.DomainEvents.Conversation;

public record ConversationCreatedEvent(string ConversationId, string ConversationName, IEnumerable<string> MemberIds) : IDomainEvent;