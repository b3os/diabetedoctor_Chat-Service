namespace ChatService.Persistence.Repositories;

public class GroupRepository : RepositoryBase<Group>, IGroupRepository
{
    public GroupRepository(MongoDbContext context) : base(context)
    {
    }
}