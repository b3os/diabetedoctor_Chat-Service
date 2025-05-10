using System.Collections;

namespace ChatService.Persistence.Repositories;

public class GroupRepository(IMongoDbContext context) : RepositoryBase<Group>(context), IGroupRepository
{
}