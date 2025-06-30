namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record JoinGroupCommand(ObjectId? ConversationId, string? UserId, string InvitedBy) : ICommand<Response>;