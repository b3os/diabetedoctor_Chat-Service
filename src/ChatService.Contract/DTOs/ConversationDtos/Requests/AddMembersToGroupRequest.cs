namespace ChatService.Contract.DTOs.ConversationDtos.Requests;

public record AddMembersToGroupRequest(HashSet<string> UserIds);