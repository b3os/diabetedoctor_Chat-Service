using ChatService.Contract.DTOs.ValueObjectDtos;

namespace ChatService.Application.Mapping;

public static class HospitalIdMapper
{
    public static HospitalId ToDomain(this HospitalIdDto dto) => HospitalId.Of(dto.Id);
}