namespace ChatService.Contract.Settings;
public class AuthSetting
{
    public const string SectionName = "AuthSetting";
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string AccessSecretToken { get; init; } = null!;

}
