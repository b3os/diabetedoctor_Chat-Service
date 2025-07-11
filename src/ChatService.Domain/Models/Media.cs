using ChatService.Domain.Enums;

namespace ChatService.Domain.Models;

public class Media : DomainEntity<ObjectId>
{
    [BsonElement("public_id")] 
    public string PublicId { get; private set; } = null!;
    
    [BsonElement("public_url")]
    public string PublicUrl { get; private set; } = null!;
    
    [BsonElement("original_file_name")]
    public string OriginalFileName { get; private set; } = null!;
    
    [BsonElement("type")]
    public MediaType MediaType { get; private set; }
 
    [BsonElement("upload_by")]
    public UserId UploadBy { get; private set; } = null!;
    
    [BsonElement("is_used")]
    public bool IsUsed { get; private set; }
    
    [BsonElement("expires_at")]
    public DateTime? ExpiresAt { get; private set; }
    
    public static Media Create(ObjectId id, string publicId, string publicUrl, string fileName, MediaType mediaTypeType, UserId uploadBy)
    {
        return new Media
        {
            Id = id,
            PublicId = publicId,
            PublicUrl = publicUrl,
            OriginalFileName = fileName,
            MediaType = mediaTypeType,
            UploadBy = uploadBy,
            IsUsed = false,
            ExpiresAt = CurrentTimeService.GetCurrentTime().AddHours(1),
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public void Use()
    {
        IsUsed = true;
        ExpiresAt = null;
    }
}