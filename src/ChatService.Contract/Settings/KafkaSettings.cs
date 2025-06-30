namespace ChatService.Contract.Settings;

public class KafkaSettings
{
    public const string SectionName = "KafkaSettings";
    
    public string BootstrapServer { get; init; } = null!;
    
    public string SaslUsername { get; init; } = null!;
    
    public string SaslPassword { get; init; } = null!;
    
    public string SourceName { get; init; } = null!;
    
    public string DeadTopic { get; init; } = null!;
    
    public string RetryTopic { get; init; } = null!;
    public string RetryConnectionName { get; init; } = null!;
    public string RetryTopicConsumerGroup { get; init; } = null!;
    
    public string ChatTopic { get; init; } = null!;
    public string ChatConnectionName { get; init; } = null!;
    public string ChatTopicConsumerGroup { get; init; } = null!;
    
    public string UserTopic { get; init; } = null!;
    public string UserConnectionName { get; init; } = null!;
    public string UserTopicConsumerGroup { get; init; } = null!;

    public string ConversationTopic { get; init; } = null!;
}