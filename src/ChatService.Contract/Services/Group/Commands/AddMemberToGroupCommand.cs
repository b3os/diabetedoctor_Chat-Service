using System.Text.Json.Serialization;
using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.DTOs.UserDTOs;
using MongoDB.Bson;

namespace ChatService.Contract.Services.Group.Commands;

public record AddMemberToGroupCommand : ICommand<Response<DuplicatedUserDto>>
{
    public string? AdminId { get; init; }
    public ObjectId GroupId { get; init; }
    public HashSet<string> UserIds { get; init; } = [];
}