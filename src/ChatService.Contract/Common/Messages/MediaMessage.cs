namespace ChatService.Contract.Common.Messages;

public enum MediaMessage
{
    [Message("Upload file thành công", "media01")]
    UploadMediaSuccessfully,
    
    [Message("File bị lỗi hoặc không tìm thấy", "media_error_01")]
    MediaNotFound,
}