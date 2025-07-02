using ChatService.Contract.Services.User.Responses;

namespace ChatService.Contract.Services.User.Queries;

public record GetAvailableUsersForConversationQuery(string UserId, ObjectId GroupId, QueryFilter Filter) : IQuery<GetAvailableUsersResponse>;