using System.ComponentModel.DataAnnotations;
using ChatService.Contract.Abstractions.Message;
using ChatService.Contract.Common.Filters;
using ChatService.Contract.DTOs.ValueObjectDtos;
using ChatService.Contract.Enums;
using ChatService.Contract.EventBus.Abstractions;
using ChatService.Contract.EventBus.Events.UserIntegrationEvents;
using ChatService.Contract.Services;
using ChatService.Contract.Services.User.Commands;
using ChatService.Contract.Services.User.Queries;
using FluentValidation;

namespace ChatService.Presentation.V1;

public static class UserEndpoints
{
    public const string ApiName = "users";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapUserApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var users = builder.MapGroup(BaseUrl).HasApiVersion(1);

        users.MapGet("", GetAvailableUsers);
        // users.MapPost("", CreateUser);
        // group.MapPatch("", UpdateUser);
        // users.MapPost("/user/{objectId}", Test);

        return builder;
    }

    // private static async Task UpdateUser(ISender sender)
    // {
    //     var userid = "b93d6316-be4c-4885-a5e0-eae1ea3d1379";
    //     var result = await sender.Send(new UpdateUserCommand {Id = userid, FullName = "Nguyễn Đỗ Chung Quý"});
    // }

    private static async Task<IResult> GetAvailableUsers(ISender sender, IClaimsService claimsService,
        [FromQuery, Required] ObjectId conversationId, [FromQuery, Required] RoleEnum role, [AsParameters] QueryOffsetFilter filters)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new GetAvailableUsersForConversationQuery()
        {
            UserId = userId,
            ConversationId = conversationId,
            OffsetFilter = filters,
            Role = role
        });
        return Results.Ok(result.Value);
    } 
    
    private static async Task<IResult> CreateUser(ISender sender, IEventPublisher eventPublisher)
    {
        var test = new CreateUserCommand
        {
            Id = "d9af5b42-f881-4de1-9ae3-08f0644d2da2",
            FullName = new FullNameDto(){FirstName = "aaaa", LastName = "bbbb"},
            Avatar = "https://pin.it/31iFcs4aU",
            PhoneNumber = "0987654321",
            Role = 1
        };
        await sender.Send(test);
        return Results.Ok();
    } 
}