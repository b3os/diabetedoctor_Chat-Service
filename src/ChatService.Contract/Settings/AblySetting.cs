namespace ChatService.Contract.Settings;

public class AblySetting
{
    public const string SectionName = "AblySetting";
    public string ApiKey { get; set; } = default!;
}