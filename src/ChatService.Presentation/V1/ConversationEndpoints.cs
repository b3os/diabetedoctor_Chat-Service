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
        conversation.MapPost("{conversationId}/members", AddMembersToGroup).RequireAuthorization().WithSummary("Add new members to the group");
        conversation.MapPost("{conversationId}/doctors", AddDoctorToGroup).RequireAuthorization().WithSummary("Add new doctor to the group");

        // update
        conversation.MapPatch("{conversationId}", UpdateConversation).RequireAuthorization().WithSummary("Update a group");
        conversation.MapPatch("{conversationId}/members/{userId}", PromoteGroupMember).RequireAuthorization().WithSummary("Promote a group member to admin");

        // delete
        conversation.MapDelete("{conversationId}", DeleteConversation).RequireAuthorization().WithSummary("Delete a group (executor is owner)");
       
        // query (get)
        conversation.MapGet("", GetUserConversation).RequireAuthorization().WithSummary("Get groups of a user");

        return builder;
    }
    
    private static async Task<IResult> CreateConversation(ISender sender, IClaimsService claimsService,
        [FromBody] CreateGroupConversationCommand command)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(command with { OwnerId = ownerId, Members = command.Members.Append(ownerId).ToHashSet() });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> UpdateConversation(ISender sender, IClaimsService claimsService, ObjectId conversationId,
        [FromBody] ConversationUpdateDto dto)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new UpdateGroupConversationCommand
        {
            AdminId = ownerId,
            ConversationId = conversationId, 
            Name = dto.Name, 
            AvatarId = dto.AvatarId
        });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> DeleteConversation(ISender sender, IClaimsService claimsService, ObjectId conversationId)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new DeleteGroupConversationCommand(OwnerId: ownerId, ConversationId: conversationId));
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> AddMembersToGroup(ISender sender, IClaimsService claimsService, ObjectId conversationId,
        [FromBody] GroupAddMembersDto dto)
    {
        var adminId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new AddMembersToGroupCommand
        {
            AdminId = adminId,
            ConversationId = conversationId,
            UserIds = dto.UserIds
        });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> AddDoctorToGroup(ISender sender, IClaimsService claimsService, ObjectId conversationId,
        [FromBody] GroupAddDoctorDto dto)
    {
        var adminId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new AddDoctorToGroupCommand(adminId, dto.DoctorId, conversationId));
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> PromoteGroupMember(ISender sender, IClaimsService claimsService, ObjectId conversationId,
        string userId)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new PromoteGroupMemberCommand
        {
            OwnerId = ownerId,
            GroupId = conversationId, 
            MemberId = userId
        });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> GetUserConversation(ISender sender, IClaimsService claimsService,
        [AsParameters] QueryFilter filter)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new GetUserConversationsByUserIdQuery() { UserId = userId, Filter = filter });
        return Results.Ok(result);
    }
}