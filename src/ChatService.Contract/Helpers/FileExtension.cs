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
    
    public static MediaTypeEnum DetectFromExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return Map.TryGetValue(ext, out var result) ? result : MediaTypeEnum.Raw;
    }
    
    private static readonly Dictionary<string, MediaTypeEnum> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = MediaTypeEnum.Image,
        [".jpeg"] = MediaTypeEnum.Image,
        [".png"] = MediaTypeEnum.Image,
        [".gif"] = MediaTypeEnum.Image,
        [".mp4"] = MediaTypeEnum.Video,
        [".mkv"] = MediaTypeEnum.Video,
        [".avi"] = MediaTypeEnum.Video
    };
}