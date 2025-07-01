﻿using ChatService.Contract.DTOs;
using ChatService.Contract.DTOs.ValueObjectDtos;
using ChatService.Contract.Enums;

namespace ChatService.Application.Mapping;

public static class FileAttachmentMapper
{
    public static FileAttachmentDto? ToDto(this FileAttachment? attachment)
    {
        if (attachment is null) return null;

        return new FileAttachmentDto
        {
            PublicId = attachment.PublicId,
            PublicUrl = attachment.PublicUrl,
            Type = attachment.Type.ToEnum<MediaType, MediaTypeEnum>()
        };
    }
    
    public static FileAttachment? ToDomain(this FileAttachmentDto? dto)
    {
        if(dto is null) return null;
        
        var type = dto.Type.ToEnum<MediaTypeEnum, MediaType>();
        return FileAttachment.Of(dto.PublicId, dto.PublicUrl, type);
    }
}