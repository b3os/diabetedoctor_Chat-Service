namespace ChatService.Domain.Abstractions;

public interface IMongoDbContext
{
    IMongoDatabase Database { get; }
    MongoClient Client { get; }
    
    IMongoCollection<User> Users { get; }
    IMongoCollection<Message> Messages { get; }
    IMongoCollection<Conversation> Conversations { get; }
    IMongoCollection<Participant> Participants { get; }
    IMongoCollection<OutboxEvent> OutboxEvents { get; }
    IMongoCollection<OutboxEventConsumer> OutboxEventsConsumers { get; }
    
}