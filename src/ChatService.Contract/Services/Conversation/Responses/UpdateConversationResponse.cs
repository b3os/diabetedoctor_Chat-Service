namespace ChatService.Contract.Services.Conversation.Responses;

public record UpdateConversationResponse(string ConversationId, int Version);