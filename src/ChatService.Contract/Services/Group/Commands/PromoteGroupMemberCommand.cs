
namespace ChatService.Contract.Services.Group.Commands;

public record PromoteGroupMemberCommand : ICommand
{
    public string? OwnerId { get; init; }
    public ObjectId GroupId { get; init; }
    public string MemberId { get; init; } = default!;
}