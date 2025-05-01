using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Exceptions.BussinessExceptions;

public static class GroupExceptions
{
    public sealed class GroupNotFoundException() : NotFoundException(
        GroupMessage.GroupNotFound.GetMessage().Message, GroupMessage.GroupNotFound.GetMessage().Code);

    public sealed class GroupAccessDeniedException() : AuthorizeException(
        GroupMessage.GroupAccessDenied.GetMessage().Message, GroupMessage.GroupAccessDenied.GetMessage().Code);
}