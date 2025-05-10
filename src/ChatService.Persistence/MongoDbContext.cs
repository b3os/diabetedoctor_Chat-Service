using System.Data.Entity;
using ChatService.Domain.Abstractions;

namespace ChatService.Persistence;

public class MongoDbContext : IMongoDbContext
{
    public MongoDbContext(IOptions<MongoDbSetting> mongoDbSetting)
    {
        Client = new MongoClient(mongoDbSetting.Value.ConnectionString);
        Database = Client.GetDatabase(mongoDbSetting.Value.DatabaseName);
    }

    public IMongoDatabase Database { get; }
    public MongoClient Client { get; }    
    public IMongoCollection<User> Users => Database.GetCollection<User>(nameof(User));    
    public IMongoCollection<Message> Messages => Database.GetCollection<Message>(nameof(Message));
    public IMongoCollection<Group> Groups => Database.GetCollection<Group>(nameof(Group));
    public IMongoCollection<MessageReadStatus> MessageReadStatuses => Database.GetCollection<MessageReadStatus>(nameof(MessageReadStatus));
}