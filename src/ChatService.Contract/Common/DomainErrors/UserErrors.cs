using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Common.DomainErrors;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(UserMessage.UserNotFound.GetMessage().Code,
        UserMessage.UserNotFound.GetMessage().Message);
    
    public static Error MustHaveThisRole(string role) => Error.BadRequest(UserMessage.MustHaveThisRole.GetMessage().Code,
        $"Chỉ người dùng có vai trò {role} mới được thêm vào nhóm.");
}