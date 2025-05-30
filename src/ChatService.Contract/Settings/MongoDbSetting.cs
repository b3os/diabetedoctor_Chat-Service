namespace ChatService.Contract.Settings;

public class MongoDbSetting
{
    public const string SectionName = "MongoDbSettings";
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}