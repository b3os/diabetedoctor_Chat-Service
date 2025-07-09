using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Common.DomainErrors;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(UserMessage.UserNotFound.GetMessage().Code,
        UserMessage.UserNotFound.GetMessage().Message);
    
    public static Error MustHaveThisRole(string role) => Error.BadRequest(UserMessage.MustHaveThisRole.GetMessage().Code,
        $"Chỉ người dùng có vai trò {role} mới được thêm vào nhóm.");
    
    public static readonly Error DoctorNotBelongToHospital = Error.BadRequest(UserMessage.DoctorNotBelongToHospital.GetMessage().Code,
        UserMessage.DoctorNotBelongToHospital.GetMessage().Message);
    
    public static readonly Error StaffNotBelongToHospital = Error.BadRequest(UserMessage.StaffNotBelongToHospital.GetMessage().Code,
        UserMessage.StaffNotBelongToHospital.GetMessage().Message);
}