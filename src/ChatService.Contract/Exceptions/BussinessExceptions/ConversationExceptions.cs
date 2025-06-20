using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Exceptions.BussinessExceptions;

public static class ConversationExceptions
{
    public sealed class GroupNotFoundException() : NotFoundException(
        ConversationMessage.ConversationNotFound.GetMessage().Message, ConversationMessage.ConversationNotFound.GetMessage().Code);

    public sealed class GroupAccessDeniedException() : ForbiddenException(
        ConversationMessage.ConversationAccessDenied.GetMessage().Message, ConversationMessage.ConversationAccessDenied.GetMessage().Code);
    
    public sealed class CannotRemoveMemberException() : ForbiddenException(
        ConversationMessage.CannotRemoveMember.GetMessage().Message, ConversationMessage.CannotRemoveMember.GetMessage().Code);
    
    public sealed class CannotDemoteOwnerException() : ForbiddenException(
        ConversationMessage.CannotDemoteOwner.GetMessage().Message, ConversationMessage.CannotDemoteOwner.GetMessage().Code);

    // Group Member
    public sealed class GroupMemberAlreadyExistsException() : BadRequestException(
        ConversationMessage.GroupMemberAlreadyExist.GetMessage().Message, ConversationMessage.GroupMemberAlreadyExist.GetMessage().Code);
    
    public sealed class GroupMemberNotExistsException() : NotFoundException(
        ConversationMessage.GroupMemberNotExists.GetMessage().Message, ConversationMessage.GroupMemberNotExists.GetMessage().Code);
    
}