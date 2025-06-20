namespace ChatService.Contract.Settings;

public class AppDefaultSettings
{
    public const string SectionName = "AppDefaultSettings";
    public string GroupAvatarDefault { get; init; } = null!;
    public string UserAvatarDefault { get; init; } = null!;
}