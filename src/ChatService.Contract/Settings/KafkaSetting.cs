namespace ChatService.Contract.Settings;

public class KafkaSetting
{
    public const string SectionName = "KafkaSetting";
    public string BootstrapServer { get; init; } = null!;
}