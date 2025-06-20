using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Common.DomainErrors;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(UserMessage.UserNotFound.GetMessage().Code,
        UserMessage.UserNotFound.GetMessage().Message);
}