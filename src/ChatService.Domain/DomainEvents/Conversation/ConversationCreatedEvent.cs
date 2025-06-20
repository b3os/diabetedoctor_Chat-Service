namespace ChatService.Domain.DomainEvents.Conversation;

public record ConversationCreatedEvent(ObjectId ConversationId, UserId OwnerId, List<User> Users) : IDomainEvent;