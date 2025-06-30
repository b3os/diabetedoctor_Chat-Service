using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Common.DomainErrors;

public class MessageErrors
{
    // Validation
    public static readonly Error CannotDetermineFile = Error.Validation(MessageMessage.CannotDetermineFile.GetMessage().Code,
        MessageMessage.CannotDetermineFile.GetMessage().Message);
}