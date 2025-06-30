namespace ChatService.Domain.DomainEvents.Conversation;

public record GroupMemberRemovedEvent(string ConversationId, string MemberId) : IDomainEvent;