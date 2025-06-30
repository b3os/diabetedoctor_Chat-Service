namespace ChatService.Contract.Common.Constraint;

public static class KafkaTopicConstraints
{
    public const string DeadTopic = "chat_dead_topic";
    
    public const string RetryTopic = "chat_retry_topic";
    public const string RetryTopicConsumerGroup = "chat_retry_topic_consumergroup";
    
    public const string ChatTopic = "chat_topic";
    public const string ChatServiceChatConsumerGroup = "chat_service-chat_group";
    
    public const string UserTopic = "user_topic";
    public const string ChatServiceUserConsumerGroup = "chat_service-user_group";

    public const string ConversationTopic = "conversation_topic";
}