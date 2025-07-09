using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Common.DomainErrors;

public static class HospitalErrors
{
    public static readonly Error HospitalNotFound = Error.BadRequest(
        HospitalMessage.HospitalNotFound.GetMessage().Code, 
        HospitalMessage.HospitalNotFound.GetMessage().Message);
}