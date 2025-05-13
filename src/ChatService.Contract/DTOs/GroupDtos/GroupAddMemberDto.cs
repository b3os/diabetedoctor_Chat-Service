namespace ChatService.Contract.DTOs.GroupDtos;

public record GroupAddMemberDto
{
    public HashSet<string> UserIds { get; init; } = [];
}