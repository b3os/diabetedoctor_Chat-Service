namespace ChatService.Persistence.Repositories;

public class MessageRepository(IMongoDbContext context) : RepositoryBase<Message>(context), IMessageRepository;