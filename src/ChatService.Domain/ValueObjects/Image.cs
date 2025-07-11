namespace ChatService.Domain.ValueObjects;

public sealed class Image : ValueObject
{
    [BsonElement("public_id")]
    public string PublicId { get; private init;} = null!;
    
    [BsonElement("public_url")]
    public string PublicUrl { get; private init;} = null!;

    private Image() {}
    
    private Image(string publicId, string publicUrl)
    {
        PublicId = publicId;
        PublicUrl = publicUrl;
    }
    
    public static Image Of(string publicId, string publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("PublicId bắt buộc phải có");
        
        if (!Uri.IsWellFormedUriString(publicUrl, UriKind.Absolute))
            throw new ArgumentException("PublicUrl không phù hợp");

        return new Image (publicId.Trim(), publicUrl.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PublicId;
        yield return PublicUrl;
    }
    
    public override string ToString() => PublicUrl;
}