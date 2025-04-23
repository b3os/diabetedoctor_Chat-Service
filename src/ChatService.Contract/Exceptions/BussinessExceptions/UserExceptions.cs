using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Exceptions.BussinessExceptions;

public static class UserExceptions
{
    public sealed class UserNotFoundException() : NotFoundException(UserMessage.UserNotFound.GetMessage().Message,
        UserMessage.UserNotFound.GetMessage().Code);
}