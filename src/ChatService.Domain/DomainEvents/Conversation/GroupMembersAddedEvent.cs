namespace ChatService.Domain.DomainEvents.Conversation;

public record GroupMembersAddedEvent(ObjectId ConversationId, UserId InvitedBy, List<User> Users) : IDomainEvent;