using System.Text.Json.Serialization;
using ChatService.Contract.DTOs.GroupDtos;
using MongoDB.Bson;

namespace ChatService.Contract.Services.Group.Commands;

public record AddMemberToGroupCommand : ICommand
{
    public string? AdminId { get; init; }
    public string? GroupId { get; init; }
    public IEnumerable<string> UserIds { get; init; } = [];
}