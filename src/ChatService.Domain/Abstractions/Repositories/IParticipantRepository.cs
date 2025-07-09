namespace ChatService.Domain.Abstractions.Repositories;

public interface IParticipantRepository : IRepositoryBase<Participant>
{
    Task<List<BsonDocument>> CheckDuplicatedParticipantsAsync(ObjectId conversationId, IEnumerable<string> userIds,
        CancellationToken cancellationToken = default);
    
    Task<UpdateResult> RejoinToConversationAsync(IClientSessionHandle session, ObjectId? conversationId, IEnumerable<UserId> participantIds,
        CancellationToken cancellationToken = default);
    
    // Task<User?> GetParticipantInfo(ObjectId conversationId, string participantId, CancellationToken cancellationToken = default);
}