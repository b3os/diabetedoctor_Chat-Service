using ChatService.Contract.Common.Filters;
using ChatService.Contract.DTOs.ConversationDtos.Requests;
using ChatService.Contract.Services;
using ChatService.Contract.Services.Conversation.Commands.GroupConversation;
using ChatService.Contract.Services.Conversation.Queries;

namespace ChatService.Presentation.V1;

public static class ConversationEndpoints
{
    public const string ApiName = "conversations";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapConversationApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var conversation = builder.MapGroup(BaseUrl).HasApiVersion(1);

        // create
        conversation.MapPost("", CreateConversation).RequireAuthorization().WithSummary("Create a new group");

        // add members
        conversation.MapPost("{conversationId}/members", AddMembersToGroup).RequireAuthorization()
            .WithSummary("Add new members to the group");
        conversation.MapPost("{conversationId}/doctors", AddDoctorToGroup).RequireAuthorization()
            .WithSummary("Add new doctor to the group");
        conversation.MapPost("{conversationId}/join", JoinGroup).RequireAuthorization()
            .WithSummary("Join a group by link (just patient)");

        // update
        conversation.MapPatch("{conversationId}", UpdateConversation).RequireAuthorization()
            .WithSummary("Update a group");

        // delete
        conversation.MapDelete("{conversationId}", DeleteConversation).RequireAuthorization()
            .WithSummary("Delete a group (executor is owner)");

        // query (get)
        conversation.MapGet("", GetUserConversation).RequireAuthorization().WithSummary("Get groups of a user");

        return builder;
    }

    private static async Task<IResult> CreateConversation(ISender sender, IClaimsService claimsService,
        [FromBody] CreateGroupConversationRequest request)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var command = new CreateGroupConversationCommand
        {
            OwnerId = ownerId,
            Name = request.Name,
            Members = request.Members.Append(ownerId).ToHashSet()
        };
        var result = await sender.Send(command);
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }

    private static async Task<IResult> UpdateConversation(ISender sender, IClaimsService claimsService,
        ObjectId conversationId,
        [FromBody] UpdateGroupConversationRequest request)
    {
        var adminId = claimsService.GetCurrentUserId;
        var command = new UpdateGroupConversationCommand
        {
            StaffId = adminId,
            ConversationId = conversationId,
            Name = request.Name,
            AvatarId = request.AvatarId
        };
        var result = await sender.Send(command);
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }

    private static async Task<IResult> DeleteConversation(ISender sender, IClaimsService claimsService,
        ObjectId conversationId)
    {
        var adminId = claimsService.GetCurrentUserId;
        var command = new DeleteGroupConversationCommand
        {
            StaffId = adminId,
            ConversationId = conversationId
        };
        var result = await sender.Send(command);
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }

    private static async Task<IResult> AddMembersToGroup(ISender sender, IClaimsService claimsService,
        ObjectId conversationId,
        [FromBody] AddMembersToGroupRequest request)
    {
        var adminId = claimsService.GetCurrentUserId;
        var command = new AddMembersToGroupCommand
        {
            StaffId = adminId,
            ConversationId = conversationId,
            UserIds = request.UserIds
        };
        var result = await sender.Send(command);
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }

    private static async Task<IResult> AddDoctorToGroup(ISender sender, IClaimsService claimsService,
        ObjectId conversationId,
        [FromBody] AddDoctorToGroupRequest request)
    {
        var adminId = claimsService.GetCurrentUserId;
        var command = new AddDoctorToGroupCommand
        {
            StaffId = adminId,
            ConversationId = conversationId,
            DoctorId = request.DoctorId
        };
        var result = await sender.Send(command);
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }

    private static async Task<IResult> JoinGroup(ISender sender, IClaimsService claimsService, ObjectId conversationId,
        [FromBody] JoinGroupRequest request)
    {
        var userId = claimsService.GetCurrentUserId;
        var command = new JoinGroupCommand
        {
            UserId = userId,
            ConversationId = conversationId,
            InvitedBy = userId
        };
        var result = await sender.Send(command);
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }

    private static async Task<IResult> GetUserConversation(ISender sender, IClaimsService claimsService,
        [AsParameters] QueryCursorFilter cursorFilter)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new GetUserConversationsByUserIdQuery() { UserId = userId, CursorFilter = cursorFilter });
        return Results.Ok(result.Value);
    }
}