namespace ChatService.Contract.Common.Messages;

public enum HospitalMessage
{
    [Message("Bệnh viện không tồn tại hoặc đã bị xóa.", "hospital_error_01")] 
    HospitalNotFound,
}