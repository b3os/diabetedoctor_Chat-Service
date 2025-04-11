using ChatService.Contract.Common.Messages;
using ChatService.Contract.Exceptions;
using ChatService.Contract.Helpers;

namespace ChatService.Contract.Exceptions.BussinessExceptions;

public static class HospitalException
{
    public sealed class HospitalNameExistException : BadRequestException
    {
        public HospitalNameExistException()
                : base(HospitalMessage.HospitalNameExistException.GetMessage().Message,
                    HospitalMessage.HospitalNameExistException.GetMessage().Code)
        { }
    }
}
