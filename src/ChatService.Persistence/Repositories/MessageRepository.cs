namespace ChatService.Persistence.Repositories;

public class MessageRepository : RepositoryBase<Message>, IMessageRepository
{
    public MessageRepository(MongoDbContext context) : base(context)
    {
    }
}