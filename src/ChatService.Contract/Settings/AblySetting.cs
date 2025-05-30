namespace ChatService.Contract.Settings;

public class AblySetting
{
    public const string SectionName = "AblySettings";
    public string ApiKey { get; set; } = null!;
}