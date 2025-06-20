namespace ChatService.Contract.Settings;

public class KafkaSettings
{
    public const string SectionName = "KafkaSettings";
    public string BootstrapServer { get; init; } = null!;
    public string SaslUsername { get; init; } = null!;
    public string SaslPassword { get; init; } = null!;
    public string SourceName { get; init; } = null!;
}