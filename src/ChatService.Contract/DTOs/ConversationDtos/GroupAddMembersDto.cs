namespace ChatService.Contract.DTOs.ConversationDtos;

public record GroupAddMembersDto
{
    public HashSet<string> UserIds { get; init; } = [];
}