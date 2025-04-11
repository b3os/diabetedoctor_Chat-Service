using ChatService.Contract.DTOs.MediaDTOs;

namespace ChatService.Contract.DTOs.UserDTOs;

public record UserDTO
    (Guid? Id,
    string Email = "",
    string FullName = "",
    string Gender = "",
    string IsActive = "",
    ImageDTO? Avatar = null);

