namespace ChatService.Contract.Common.Constraint;

public static class AblyTopicConstraints
{
    public const string MessageReadChannel = "message-read-latest";
    public const string MessageReadEvent = "last-read-message";
    public const string GlobalChatChannel = "global-chat";
    public const string GlobalChatEvent = "global-received-message";
}