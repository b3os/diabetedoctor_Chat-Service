namespace ChatService.Contract.Common.Messages;

public enum UserMessage
{
    //Exception
    [Message("Người dùng không tồn tại", "user01")]
    UserNotFound,
}
