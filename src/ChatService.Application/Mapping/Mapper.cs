using ChatService.Contract.DTOs;
using ChatService.Contract.DTOs.ValueObjectDtos;

namespace ChatService.Application.Mapping;

public static class Mapper
{
    /// <summary>
    /// Map DTO sang ValueObject UserId.
    /// </summary>
    public static UserId MapUserId(UserIdDto dto)
    {
        return dto.ToDomain();
    }

    public static FileAttachment? MapFileAttachment(FileAttachmentDto? dto)
    {
        return dto.ToDomain();
    }
    
    public static FileAttachmentDto? MapFileAttachmentDto(FileAttachment? attachment)
    {
        return attachment.ToDto();
    }
}