namespace ChatService.Domain.DomainEvents.Conversation;

public record GroupMembersAddedEvent(string ConversationId, IEnumerable<string> Members) : IDomainEvent;