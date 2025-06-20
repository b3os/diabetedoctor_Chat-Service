using ChatService.Contract.Services.Message.Responses;

namespace ChatService.Contract.Services.Message.Queries;

public record GetMessageByConversationIdQuery : IQuery<GetMessagesResponse>
{
    public ObjectId ConversationId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public QueryFilter Filter { get; init; } = new();
};