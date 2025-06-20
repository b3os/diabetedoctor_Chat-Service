namespace ChatService.Contract.DTOs.ConversationDtos;

public record GroupAddMemberDto
{
    public HashSet<string> UserIds { get; init; } = [];
}