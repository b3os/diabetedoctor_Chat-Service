namespace ChatService.Contract.Settings;

public class MongoDbSetting
{
    public const string SectionName = "MongoDbSetting";
    public string ConnectionString { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
}