
namespace ChatService.Domain.Abstractions.Repositories;

public interface IGroupRepository : IRepositoryBase<Group>
{
    Task<BsonDocument?> CheckDuplicatedUsersAsync(ObjectId groupId, IEnumerable<string> userIds, CancellationToken cancellationToken = default);
    Task<BsonDocument?> GetGroupMemberInfoAsync(ObjectId groupId, string memberId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetExecutorAndTarget(ObjectId groupId, string executorId, string targetId, CancellationToken cancellationToken = default);
}