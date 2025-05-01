
namespace ChatService.Domain.Abstractions.Repositories;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<UpdateResult> UpdateUserAsync(IClientSessionHandle session, string id, User user, CancellationToken cancellationToken = default);
}