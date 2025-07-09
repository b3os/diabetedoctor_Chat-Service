namespace ChatService.Contract.DTOs.ConversationDtos.Requests;

public record CreateGroupConversationRequest(string Name, HashSet<string> Members);