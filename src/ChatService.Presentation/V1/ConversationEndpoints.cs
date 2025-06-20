using ChatService.Contract.Services;
using ChatService.Contract.Services.Conversation.Queries;

namespace ChatService.Presentation.V1;

public static class ConversationEndpoints
{
    public const string ApiName = "conversations";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";
    
    public static IVersionedEndpointRouteBuilder MapConversationApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var conversation = builder.MapGroup(BaseUrl).HasApiVersion(1);

        conversation.MapPost("", CreateConversation).RequireAuthorization().WithSummary("Create a new group");
        conversation.MapPost("{groupId}/members", AddMemberToGroup).RequireAuthorization().WithSummary("Add new members to the group");
        
        conversation.MapPatch("{groupId}", UpdateConversation).RequireAuthorization().WithSummary("Update a group");
        conversation.MapPatch("{groupId}/members/{userId}", PromoteGroupMember).RequireAuthorization().WithSummary("Promote a group member to admin");

        conversation.MapDelete("{groupId}", DeleteConversation).RequireAuthorization().WithSummary("Delete a group (executor is owner)");
       
        conversation.MapGet("", GetUserConversation).RequireAuthorization().WithSummary("Get groups of a user");

        return builder;
    }
    
    private static async Task<IResult> CreateConversation(ISender sender, IClaimsService claimsService,
        [FromBody] CreateConversationCommand command)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(command with { OwnerId = ownerId, Members = command.Members.Append(ownerId).ToHashSet() });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> UpdateConversation(ISender sender, IClaimsService claimsService, ObjectId groupId,
        [FromBody] ConversationUpdateDto dto)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new UpdateConversationCommand
        {
            AdminId = ownerId,
            GroupId = groupId, 
            Name = dto.Name, 
            Avatar = dto.Avatar, 
            Version = dto.Version
        });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> DeleteConversation(ISender sender, IClaimsService claimsService, ObjectId groupId)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new DeleteConversationCommand(OwnerId: ownerId, ConversationId: groupId));
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> AddMemberToGroup(ISender sender, IClaimsService claimsService, ObjectId groupId,
        [FromBody] GroupAddMemberDto dto)
    {
        var adminId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new AddMemberToGroupCommand
        {
            AdminId = adminId,
            ConversationId = groupId,
            UserIds = dto.UserIds
        });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }
    
    private static async Task<IResult> PromoteGroupMember(ISender sender, IClaimsService claimsService, ObjectId groupId,
        string userId)
    {
        var ownerId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new PromoteGroupMemberCommand
        {
            OwnerId = ownerId,
            GroupId = groupId, 
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