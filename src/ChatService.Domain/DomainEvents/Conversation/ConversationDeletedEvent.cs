
namespace ChatService.Domain.DomainEvents.Conversation;

public record ConversationDeletedEvent(string ConversationId) : IDomainEvent;