namespace ChatService.Domain.DomainEvents.Conversation;

public record GroupDoctorAddedEvent(ObjectId ConversationId, UserId InvitedBy, UserId DoctorId) : IDomainEvent
{
    
}