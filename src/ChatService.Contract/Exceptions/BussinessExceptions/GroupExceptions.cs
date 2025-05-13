using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Exceptions.BussinessExceptions;

public static class GroupExceptions
{
    public sealed class GroupNotFoundException() : NotFoundException(
        GroupMessage.GroupNotFound.GetMessage().Message, GroupMessage.GroupNotFound.GetMessage().Code);

    public sealed class GroupAccessDeniedException() : ForbiddenException(
        GroupMessage.GroupAccessDenied.GetMessage().Message, GroupMessage.GroupAccessDenied.GetMessage().Code);
    
    public sealed class CannotRemoveMemberException() : ForbiddenException(
        GroupMessage.CannotRemoveMember.GetMessage().Message, GroupMessage.CannotRemoveMember.GetMessage().Code);
    
    public sealed class CannotDemoteOwnerException() : ForbiddenException(
        GroupMessage.CannotDemoteOwner.GetMessage().Message, GroupMessage.CannotDemoteOwner.GetMessage().Code);

    // Group Member
    public sealed class GroupMemberAlreadyExistsException() : BadRequestException(
        GroupMessage.GroupMemberAlreadyExists.GetMessage().Message, GroupMessage.GroupMemberAlreadyExists.GetMessage().Code);
    
    public sealed class GroupMemberNotExistsException() : NotFoundException(
        GroupMessage.GroupMemberNotExists.GetMessage().Message, GroupMessage.GroupMemberNotExists.GetMessage().Code);
    
}