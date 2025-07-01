namespace ChatService.Contract.Settings;
public class AuthSettings
{
    public const string SectionName = "AuthSettings";
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string AccessSecretToken { get; init; } = null!;

}
