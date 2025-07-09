using ChatService.Contract.Common.Filters;
using ChatService.Contract.Services.Message.Responses;

namespace ChatService.Contract.Services.Message.Queries;

public record GetMessageByConversationIdQuery : IQuery<GetMessagesResponse>
{
    public ObjectId ConversationId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public QueryCursorFilter CursorFilter { get; init; } = new();
};