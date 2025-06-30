namespace ChatService.Contract.Common.Messages;

public enum MessageMessage
{
    [Message("Lưu tin nhắn thành công", "message01")]
    CreateMessageSuccessfully,
    
    [Message("Không tìm thấy tệp đính kèm", "message_validation_01")]
    CannotDetermineFile,
}