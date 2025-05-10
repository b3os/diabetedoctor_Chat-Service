namespace ChatService.Domain.Abstractions;

public interface IMongoDbContext
{
    IMongoDatabase Database { get; }
    MongoClient Client { get; }
    
    IMongoCollection<User> Users { get; }
    IMongoCollection<Message> Messages { get; }
    IMongoCollection<Group> Groups { get; }
    IMongoCollection<MessageReadStatus> MessageReadStatuses { get; }
}