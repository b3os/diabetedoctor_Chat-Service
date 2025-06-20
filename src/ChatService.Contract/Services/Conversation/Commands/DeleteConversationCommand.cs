namespace ChatService.Contract.Services.Conversation.Commands;

public record DeleteConversationCommand(string OwnerId, ObjectId ConversationId) : ICommand<Response>;