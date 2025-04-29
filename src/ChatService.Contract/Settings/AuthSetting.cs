namespace ChatService.Contract.Settings;
public class AuthSetting
{
    public const string SectionName = "AuthSetting";
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public string AccessSecretToken { get; init; } = default!;

}
