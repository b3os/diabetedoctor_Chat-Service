namespace ChatService.Persistence;

public class MongoDbContext
{
    public MongoDbContext(IOptions<MongoDbSetting> mongoDbSetting)
    {
        var mongoClient = new MongoClient(mongoDbSetting.Value.ConnectionString);
        Database = mongoClient.GetDatabase(mongoDbSetting.Value.DatabaseName);
    }

    public IMongoDatabase Database { get; }
    
}