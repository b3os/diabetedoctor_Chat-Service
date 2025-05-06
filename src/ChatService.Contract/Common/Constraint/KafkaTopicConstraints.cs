namespace ChatService.Contract.Common.Constraint;

public static class KafkaTopicConstraints
{
    public const string ChatTopic = "chat_topic";
    public const string UserTopic = "user_topic";
    public const string ChatServiceUserConsumerGroup = "chat_service-user_group";
}