namespace ChatService.Persistence.Repositories;

public class MessageReadStatusRepository(MongoDbContext context)
    : RepositoryBase<MessageReadStatus>(context), IMessageReadStatusRepository
{
    
}