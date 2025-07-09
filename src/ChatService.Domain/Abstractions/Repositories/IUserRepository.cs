
namespace ChatService.Domain.Abstractions.Repositories;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<BsonDocument?> GetUserWithHospital(UserId userId, CancellationToken cancellationToken);
}