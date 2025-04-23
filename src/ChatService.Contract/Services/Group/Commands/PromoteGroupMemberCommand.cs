namespace ChatService.Contract.Services.Group.Commands;

public record PromoteGroupMemberCommand : ICommand
{
    public required string GroupId { get; init; }
    public required string MemberId { get; init; }
};