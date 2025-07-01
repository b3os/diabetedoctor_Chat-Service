namespace ChatService.Contract.Settings;

public class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";
    public string ConnectionString { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
}