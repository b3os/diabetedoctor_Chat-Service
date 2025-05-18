using MongoDB.Bson;

namespace ChatService.Contract.Services.Group.Commands;

public record RemoveMemberFromGroupCommand : ICommand
{
    public string? AdminId { get; init; }
    public ObjectId GroupId { get; init; }
    public string MemberId { get; init; } = null!;
}