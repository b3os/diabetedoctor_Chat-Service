
using ChatService.Domain.Abstractions;

namespace ChatService.Persistence.Repositories;

public class MessageReadStatusRepository(IMongoDbContext context)
    : RepositoryBase<MessageReadStatus>(context), IMessageReadStatusRepository
{
    
}