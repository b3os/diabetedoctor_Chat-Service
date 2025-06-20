using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Exceptions.BussinessExceptions;

public static class ParticipantExceptions
{
    public sealed class ParticipantNotFoundException() : NotFoundException(
        ParticipantMessage.ParticipantNotFound.GetMessage().Message, ParticipantMessage.ParticipantNotFound.GetMessage().Code);
}