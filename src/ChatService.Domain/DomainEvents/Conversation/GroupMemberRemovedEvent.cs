namespace ChatService.Domain.DomainEvents.Conversation;

public record GroupMemberRemovedEvent(ObjectId ParticipantId) : IDomainEvent;