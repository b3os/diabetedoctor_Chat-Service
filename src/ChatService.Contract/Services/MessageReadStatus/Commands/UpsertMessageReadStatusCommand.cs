using MongoDB.Bson;

namespace ChatService.Contract.Services.MessageReadStatus.Commands;

public record UpsertMessageReadStatusCommand : ICommand
{
    public string UserId { get; init; } = default!;
    public ObjectId MessageId { get; init; }
    public ObjectId GroupId { get; init; } = default!;
}