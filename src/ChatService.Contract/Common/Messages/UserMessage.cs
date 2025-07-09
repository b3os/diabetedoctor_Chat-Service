namespace ChatService.Contract.Common.Messages;

public enum UserMessage
{
    //Exception
    [Message("Người dùng không tồn tại hoặc đã bị cấm khỏi hệ thống", "user01")]
    UserNotFound,
    
    [Message("Role không phù hợp", "user02")]
    MustHaveThisRole,
    
    [Message("Bác sĩ không trực thuộc bệnh viện", "user02")]
    DoctorNotBelongToHospital,
    
    [Message("Nhân viên không trực thuộc bệnh viện", "user03")]
    StaffNotBelongToHospital
}
