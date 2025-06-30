namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record DeleteGroupConversationCommand(string OwnerId, ObjectId ConversationId) : ICommand<Response>;