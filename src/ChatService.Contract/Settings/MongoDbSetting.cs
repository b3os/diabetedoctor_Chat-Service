namespace ChatService.Contract.Settings;

public class MongoDbSetting
{
    public const string SectionName = "MongoDbSetting";
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}