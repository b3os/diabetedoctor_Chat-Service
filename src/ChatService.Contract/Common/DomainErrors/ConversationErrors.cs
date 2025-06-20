using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Common.DomainErrors;

public static class ConversationErrors
{
    public static readonly Error NotFound = Error.NotFound(ConversationMessage.ConversationNotFound.GetMessage().Code,
        ConversationMessage.ConversationNotFound.GetMessage().Message);
    
    public static readonly Error Forbidden = Error.Forbidden(ConversationMessage.ConversationAccessDenied.GetMessage().Code,
        ConversationMessage.ConversationAccessDenied.GetMessage().Message);
    
    public static readonly Error GroupMemberNotExists = Error.NotFound(ConversationMessage.GroupMemberNotExists.GetMessage().Code,
        ConversationMessage.GroupMemberNotExists.GetMessage().Message);
    
    public static readonly Error CannotDemoteOwner = Error.Forbidden(ConversationMessage.CannotDemoteOwner.GetMessage().Code,
        ConversationMessage.CannotDemoteOwner.GetMessage().Message);
    
    public static readonly Error CannotRemoveMember = Error.Forbidden(ConversationMessage.CannotRemoveMember.GetMessage().Code,
        ConversationMessage.CannotRemoveMember.GetMessage().Message);
    
    public static readonly Error YouNotConversationParticipant = Error.Forbidden(ConversationMessage.YouNotConversationParticipant.GetMessage().Code,
        ConversationMessage.YouNotConversationParticipant.GetMessage().Message);
    
    public static Error GroupMembersAlreadyExist(object members) => Error.Conflict(ConversationMessage.GroupMemberAlreadyExist.GetMessage().Code,
        members);
    
    // Validation
    public static readonly Error SameAsCurrentName = Error.Validation(ConversationMessage.SameAsCurrentName.GetMessage().Code,
        ConversationMessage.SameAsCurrentName.GetMessage().Message);
}