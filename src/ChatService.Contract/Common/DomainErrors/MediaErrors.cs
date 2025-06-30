using ChatService.Contract.Common.Messages;

namespace ChatService.Contract.Common.DomainErrors;

public class MediaErrors
{
    public static readonly Error MediaNotFound = Error.NotFound(MediaMessage.MediaNotFound.GetMessage().Code,
        MediaMessage.MediaNotFound.GetMessage().Message);
}