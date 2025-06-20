
namespace ChatService.Domain.DomainEvents.Conversation;

public record ConversationDeletedEvent(ObjectId ConversationId) : IDomainEvent;