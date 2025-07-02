// using ChatService.Contract.Services.User.Queries;
// using ChatService.Contract.Services.User.Responses;
//
// namespace ChatService.Application.UseCase.V1.Queries.User;
//
// public sealed class GetAvailableUsersForConversationQueryHandler(
//     IMongoDbContext mongoDbContext)
//     : IQueryHandler<GetAvailableUsersForConversationQuery, GetAvailableUsersResponse>
// {
//     public async Task<Result<GetAvailableUsersResponse>> Handle(GetAvailableUsersForConversationQuery request, CancellationToken cancellationToken)
//     {
//         var pageSize = request.Filter.PageSize is > 0 ? request.Filter.PageSize.Value : 20;
//         var checkPermissionResult = await CheckConversationPermissionAsync(request.ConversationId, request.UserId, cancellationToken);
//         if (checkPermissionResult.IsFailure)
//         {
//             return Result.Failure<GetAvailableUsersResponse>(checkPermissionResult.Error);
//         }
//     }
//     
//     private async Task<Result> CheckConversationPermissionAsync(ObjectId conversationId,
//         string userId,
//         CancellationToken cancellationToken)
//     {
//         var isConversationExisted = await mongoDbContext.Conversations.Find(
//                 c => c.Id == conversationId
//                      && c.Members.Any(member => member.Id == userId))
//             .AnyAsync(cancellationToken);
//
//         return isConversationExisted
//             ? Result.Success()
//             : Result.Failure(ConversationErrors.NotFound);
//     }
// }