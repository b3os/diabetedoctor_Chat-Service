﻿namespace ChatService.Contract.Common.Constraint;

public static class KafkaTopicConstraints
{
    public const string DeadTopic = "chat_dead_topic";
    public const string RetryTopic = "chat_retry_topic";
    
    public const string ChatTopic = "test";
    public const string ChatServiceChatConsumerGroup = "chat_service-chat_topic";
    
    public const string UserTopic = "user_topic";
    public const string ChatServiceUserConsumerGroup = "chat_service-user_group1";

    public const string ConversationTopic = "conversation_topic";
}