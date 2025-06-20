namespace ChatService.Domain.Abstractions.Repositories;

public interface IParticipantRepository : IRepositoryBase<Participant>
{
    Task<List<User>> CheckDuplicatedParticipantAsync(ObjectId groupId, IEnumerable<string> userIds,
        CancellationToken cancellationToken = default);
    
    // Task<User?> GetParticipantInfo(ObjectId conversationId, string participantId, CancellationToken cancellationToken = default);
}