using ChatService.Contract.DTOs;

namespace ChatService.Application.Mapping;

public static class FileAttachmentMapper
{
    public static FileAttachmentDto? ToDto(this FileAttachment? attachment)
    {
        if (attachment is null) return null;

        return new FileAttachmentDto
        {
            PublicUrl = attachment.PublicUrl,
            Type = (int)attachment.Type
        };
    }
    
    public static FileAttachment? ToDomain(this FileAttachmentDto? dto)
    {
        return dto is null ? null : FileAttachment.Of(string.Empty, dto.PublicUrl, (MediaType)dto.Type);
    }
}