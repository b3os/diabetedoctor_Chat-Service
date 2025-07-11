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

    /// <summary>
    /// Map DTO sang ValueObject UserId.
    /// </summary>
    public static HospitalId MapHospitalId(HospitalIdDto dto)
    {
        return dto.ToDomain();
    }
    
    /// <summary>
    /// Map DTO sang ValueObject FileAttachment.
    /// </summary>
    public static FileAttachment? MapFileAttachment(FileAttachmentDto? dto)
    {
        return dto.ToDomain();
    }
    
    /// <summary>
    /// Map ValueObject sang DTO FileAttachmentDto.
    /// </summary>
    public static FileAttachmentDto? MapFileAttachmentDto(FileAttachment? attachment)
    {
        return attachment.ToDto();
    }

    /// <summary>
    /// Map int value sang Enum Role.
    /// </summary>
    public static Role MapRoleFromInt(int value)
    {
        return RoleMapper.MapFromInt(value);
    }
    
    /// <summary>
    /// Map DTO sang ValueObject FullName
    /// </summary>
    public static FullName MapFullName(FullNameDto dto)
    {
        return dto.ToDomain();
    }
    
    /// <summary>
    /// Map ValueObject sang DTO FullNameDto
    /// </summary>
    public static FullNameDto MapFullNameDto(FullName fullName)
    {
        return fullName.ToDto();
    }
}