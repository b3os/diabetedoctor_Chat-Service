using System.ComponentModel.DataAnnotations;
using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.Services;
using ChatService.Contract.Services.Message.Commands;
using ChatService.Contract.Services.Message.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ChatService.Presentation.V1;

public static class ChatEndpoints
{
    public const string ApiName = "chats";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapChatApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var chat = builder.MapGroup(BaseUrl).HasApiVersion(1).DisableAntiforgery();
        chat.MapPost("groups/{conversationId}/messages", CreateMessage).RequireAuthorization()
            .WithSummary("Creates a new message");
        chat.MapGet("messages", GetGroupMessages).RequireAuthorization().WithSummary("Gets all messages");
        return builder;
    }

    private static async Task<IResult> CreateMessage(ISender sender, IClaimsService claimsService, ObjectId conversationId,
        [FromBody] MessageCreateDto dto)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new CreateMessageCommand
        {
            ConversationId = conversationId, ConversationType = dto.ConversationType, UserId = userId, 
            Content = dto.Content,
            MediaId = dto.MediaId,
            MessageType = dto.MessageType
        });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();
    }

    private static async Task<IResult> GetGroupMessages(ISender sender, IClaimsService claimsService,
        [FromQuery, Required] ObjectId groupId, [AsParameters] QueryFilter filter)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new GetMessageByConversationIdQuery()
            { ConversationId = groupId, UserId = userId, Filter = filter });
        return result.IsSuccess ? Results.Ok(result.Value) : result.HandlerFailure();    
    }
    
}