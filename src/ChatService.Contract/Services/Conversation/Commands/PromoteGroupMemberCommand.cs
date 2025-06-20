namespace ChatService.Contract.Services.Conversation.Commands;

public record PromoteGroupMemberCommand : ICommand<Response>
{
    public string? OwnerId { get; init; }
    public ObjectId GroupId { get; init; }
    public string MemberId { get; init; } = null!;
}