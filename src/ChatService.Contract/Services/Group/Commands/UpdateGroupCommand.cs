using System.Text.Json.Serialization;
using ChatService.Contract.DTOs.GroupDtos;
using MongoDB.Bson;

namespace ChatService.Contract.Services.Group.Commands;

public record UpdateGroupCommand : ICommand
{
    public string? AdminId { get; init; }
    public ObjectId GroupId { get; init; }
    public string? Name { get; init; }
    public string? Avatar { get; init; }
}