namespace ChatService.Contract.Common.Messages;

public enum ParticipantMessage
{
    [Message("Thành viên không tồn tại hoặc đã bị xóa", "participant_exception_01")]
    ParticipantNotFound
}