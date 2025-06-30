namespace ChatService.Contract.Services.Conversation.Commands.GroupConversation;

public record AddDoctorToGroupCommand(string AdminId, string DoctorId, ObjectId ConversationId) : ICommand<Response>;