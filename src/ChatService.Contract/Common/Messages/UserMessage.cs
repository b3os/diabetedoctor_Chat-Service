namespace ChatService.Contract.Common.Messages;

public enum UserMessage
{
    [Message("Đã lấy thông tin người dùng thành công", "user01")]
    GetUserSuccessfully,
    [Message("Tạo thông tin người dùng thành công", "user02")]
    CreateUserSuccessfully,
}
