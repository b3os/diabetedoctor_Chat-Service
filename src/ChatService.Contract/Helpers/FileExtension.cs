using ChatService.Contract.Enums;

namespace ChatService.Contract.Helpers;

public static class FileExtension
{
    public static bool HasImageExtension(string fileName)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(ext);
    }
    
    public static MediaEnum DetectFromExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return Map.TryGetValue(ext, out var result) ? result : MediaEnum.Raw;
    }
    
    private static readonly Dictionary<string, MediaEnum> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = MediaEnum.Image,
        [".jpeg"] = MediaEnum.Image,
        [".png"] = MediaEnum.Image,
        [".gif"] = MediaEnum.Image,
        [".mp4"] = MediaEnum.Video,
        [".mkv"] = MediaEnum.Video,
        [".avi"] = MediaEnum.Video
    };
}