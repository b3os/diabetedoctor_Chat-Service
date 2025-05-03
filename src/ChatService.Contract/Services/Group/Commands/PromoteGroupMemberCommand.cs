namespace ChatService.Contract.Services.Group.Commands;

public record PromoteGroupMemberCommand : ICommand
{
    public string? OwnerId { get; init; }
    public string GroupId { get; init; } = default!;
    public string MemberId { get; init; } = default!;
}