namespace ChatService.Contract.EventBus.Events.ConversationIntegrationEvents;

public record GroupMembersAddedIntegrationEvent(string ConversationId, IEnumerable<string> Members) : IntegrationEvent;