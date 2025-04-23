namespace ChatService.Persistence;

public class MongoDbContext
{
    public MongoDbContext(IOptions<MongoDbSetting> mongoDbSetting)
    {
        Client = new MongoClient(mongoDbSetting.Value.ConnectionString);
        Database = Client.GetDatabase(mongoDbSetting.Value.DatabaseName);
    }

    public IMongoDatabase Database { get; }
    public MongoClient Client { get; }    
    public IMongoCollection<User> Users => Database.GetCollection<User>(nameof(User));    
}