using ChatService.Contract.DTOs.ValueObjectDtos;
using ChatService.Contract.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.Contract.DTOs.UserDTOs;

[BsonIgnoreExtraElements]
public record UserWithHospitalDto
{
    [BsonElement("user_id")]
    public UserIdDto UserId { get; init; } = null!;
    [BsonElement("hospital_id")]
    public HospitalIdDto? HospitalId { get; init; }
    public RoleEnum Role { get; init; }
}