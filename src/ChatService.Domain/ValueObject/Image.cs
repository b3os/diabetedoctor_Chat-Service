namespace ChatService.Domain.ValueObject;

public record Image(string PublicUrl)
{
    public static Image Of(string publicUrl)
    {
        return new Image(publicUrl);
    }
}