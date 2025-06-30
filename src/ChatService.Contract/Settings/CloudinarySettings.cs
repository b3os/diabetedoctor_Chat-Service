namespace ChatService.Contract.Settings;

public class CloudinarySettings
{
    public const string SectionName = "CloudinarySettings";
    public string Folder { get; set; } = null!;
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
}