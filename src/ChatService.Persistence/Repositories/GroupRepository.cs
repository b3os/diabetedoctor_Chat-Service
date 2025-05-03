using System.Collections;

namespace ChatService.Persistence.Repositories;

public class GroupRepository(MongoDbContext context) : RepositoryBase<Group>(context), IGroupRepository
{
}