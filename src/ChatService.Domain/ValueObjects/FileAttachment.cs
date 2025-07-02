

using ChatService.Domain.Enums;

namespace ChatService.Domain.ValueObjects;

public sealed class FileAttachment : ValueObject
{
    [BsonElement("public_id")]
    public string PublicId { get; private init; } = null!;
    
    [BsonElement("public_url")]
    public string PublicUrl { get; private init; } = null!;
    
    [BsonElement("file_type")]
    public MediaType Type { get; private init; }
    
    private FileAttachment() { }
    
    private FileAttachment(string publicId, string publicUrl, MediaType fileType)
    {
        PublicId = publicId;
        PublicUrl = publicUrl;
        Type = fileType;
    }

    public static FileAttachment Of(string publicId, string publicUrl, MediaType fileType)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("PublicId bắt buộc");
        
        if (!Uri.IsWellFormedUriString(publicUrl, UriKind.Absolute))
            throw new ArgumentException("PublicUrl không phù hợp");
        
        return new FileAttachment(publicId.Trim(), publicUrl.Trim(), fileType);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PublicId;
        yield return PublicUrl;
        yield return Type;
    }
}