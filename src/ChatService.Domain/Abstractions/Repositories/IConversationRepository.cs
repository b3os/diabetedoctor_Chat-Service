namespace ChatService.Domain.Abstractions.Repositories;

public interface IConversationRepository : IRepositoryBase<Conversation>
{
    Task<UpdateResult> UpdateConversationAsync(IClientSessionHandle session, ObjectId conversationId, string? name, Image? avatar, CancellationToken cancellationToken = default);
    Task<UpdateResult> AddMemberToConversationAsync(IClientSessionHandle session, ObjectId conversationId, List<UserId> memberIds, CancellationToken cancellationToken = default);
    Task<UpdateResult> RemoveMemberFromConversationAsync(IClientSessionHandle session, ObjectId conversationId, UserId memberId, CancellationToken cancellationToken = default);
    Task<UpdateResult> UpdateLastMessageInConversationAsync(IClientSessionHandle session, ObjectId conversationId, Message lastMessage, CancellationToken cancellationToken = default);
}