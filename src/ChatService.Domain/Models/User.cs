using ChatService.Domain.Enums;

namespace ChatService.Domain.Models;

[BsonIgnoreExtraElements]
public class User : DomainEntity<ObjectId>
{
    [BsonElement("user_id")]
    public UserId UserId { get; private set; } = null!;
    [BsonElement("hospital_id")]
    public HospitalId? HospitalId { get; private set; }
    [BsonElement("full_name")]
    public FullName FullName { get; private set; } = null!;
    [BsonElement("display_name")]
    public string DisplayName { get; private set; } = null!;
    [BsonElement("avatar")]
    public Image Avatar { get; private set; } = null!;
    [BsonElement("phone_number")]
    public string? PhoneNumber { get; private set; }
    [BsonElement("role")]
    public Role Role { get; private set; }
    
    private User() { }

    public static User Create(ObjectId id, UserId userId, HospitalId? hospitalId, FullName fullname, Image avatar, string? phoneNumber, Role role)
    {
        return new User
        {
            Id = id,
            UserId = userId,
            HospitalId = hospitalId,
            Avatar = avatar,
            FullName = fullname,
            DisplayName = fullname.ToString(),
            PhoneNumber = phoneNumber,
            Role = role,
            CreatedDate = CurrentTimeService.GetCurrentTime(),
            ModifiedDate = CurrentTimeService.GetCurrentTime(),
            IsDeleted = false
        };
    }

    public void Modify(FullName? fullname, Image? avatar, string? phoneNumber)
    {
        FullName = fullname ?? FullName;
        Avatar = avatar ?? Avatar;
        PhoneNumber = phoneNumber ?? PhoneNumber;
        ModifiedDate = CurrentTimeService.GetCurrentTime();
    }
}