namespace ChatService.Contract.Settings;

public class UserSetting
{
    public const string SectionName = "UserSetting";
    public string AvatarDefaultUrl { get; set; } = null!;
}