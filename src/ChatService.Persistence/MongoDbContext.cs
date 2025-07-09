namespace ChatService.Persistence;

public class MongoDbContext : IMongoDbContext
{
    public MongoDbContext(IOptions<MongoDbSettings> mongoDbSetting)
    {
        Client = new MongoClient(mongoDbSetting.Value.ConnectionString);
        Database = Client.GetDatabase(mongoDbSetting.Value.DatabaseName);
    }

    public IMongoDatabase Database { get; }
    public MongoClient Client { get; }    
    public IMongoCollection<User> Users => Database.GetCollection<User>(nameof(User));    
    public IMongoCollection<Message> Messages => Database.GetCollection<Message>(nameof(Message));
    public IMongoCollection<Conversation> Conversations => Database.GetCollection<Conversation>(nameof(Conversation));
    public IMongoCollection<Participant> Participants => Database.GetCollection<Participant>(nameof(Participant));
    public IMongoCollection<Media> Media => Database.GetCollection<Media>(nameof(Media));
    public IMongoCollection<Hospital> Hospitals => Database.GetCollection<Hospital>(nameof(Hospital));
    public IMongoCollection<OutboxEvent> OutboxEvents => Database.GetCollection<OutboxEvent>(nameof(OutboxEvent));
    public IMongoCollection<OutboxEventConsumer> OutboxEventConsumers => Database.GetCollection<OutboxEventConsumer>(nameof(OutboxEventConsumer));
}