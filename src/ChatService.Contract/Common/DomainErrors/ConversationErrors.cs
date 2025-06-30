using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Common.DomainErrors;

public static class ConversationErrors
{
    public static readonly Error NotFound = Error.NotFound(
        ConversationMessage.ConversationNotFound.GetMessage().Code,
        ConversationMessage.ConversationNotFound.GetMessage().Message);
    
    public static readonly Error Forbidden = Error.Forbidden(
        ConversationMessage.ConversationAccessDenied.GetMessage().Code,
        ConversationMessage.ConversationAccessDenied.GetMessage().Message);
    
    public static readonly Error GroupMemberNotExists = Error.NotFound(
        ConversationMessage.GroupMemberNotExists.GetMessage().Code,
        ConversationMessage.GroupMemberNotExists.GetMessage().Message);
    
    public static readonly Error CannotDemoteOwner = Error.Forbidden(
        ConversationMessage.CannotDemoteOwner.GetMessage().Code,
        ConversationMessage.CannotDemoteOwner.GetMessage().Message);
    
    public static readonly Error CannotRemoveMember = Error.Forbidden(
        ConversationMessage.CannotRemoveMember.GetMessage().Code,
        ConversationMessage.CannotRemoveMember.GetMessage().Message);
    
    public static readonly Error ThisConversationIsClosed = Error.Forbidden(
        ConversationMessage.ThisConversationIsClosed.GetMessage().Code,
        ConversationMessage.ThisConversationIsClosed.GetMessage().Message);
    
    public static Error GroupMembersAlreadyExistedOrBanned(object members) => Error.Conflict(
        ConversationMessage.GroupMemberAlreadyExistedOrBanned.GetMessage().Code,
        members);
    
    public static readonly Error MemberIsBanned = Error.BadRequest(
        ConversationMessage.MemberIsBanned.GetMessage().Code,
        ConversationMessage.MemberIsBanned.GetMessage().Message);
    
    public static readonly Error MemberAlreadyExisted = Error.Conflict(
        ConversationMessage.MemberAlreadyExisted.GetMessage().Code,
        ConversationMessage.MemberAlreadyExisted.GetMessage().Message);
    
    public static readonly Error YouAreBanned = Error.Forbidden(
        ConversationMessage.YouAreBanned.GetMessage().Code,
        ConversationMessage.YouAreBanned.GetMessage().Message);
    
    public static readonly Error YouAlreadyInGroup = Error.Conflict(
        ConversationMessage.YouAlreadyInGroup.GetMessage().Code,
        ConversationMessage.YouAlreadyInGroup.GetMessage().Message);
    
    // Validation
    public static readonly Error SameAsCurrentName = Error.Validation(
        ConversationMessage.SameAsCurrentName.GetMessage().Code,
        ConversationMessage.SameAsCurrentName.GetMessage().Message);
}