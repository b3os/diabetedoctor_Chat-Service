using System.ComponentModel.DataAnnotations;
using Asp.Versioning.Builder;
using ChatService.Contract.Abstractions.Shared;
using ChatService.Contract.DTOs.MessageDtos;
using ChatService.Contract.Infrastructure.Services;
using ChatService.Contract.Services;
using ChatService.Contract.Services.Message.Commands;
using ChatService.Contract.Services.Message.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Presentation.V1;

public static class ChatEndpoints
{
    public const string ApiName = "chats";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";
    
    public static IVersionedEndpointRouteBuilder MapChatApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var chat = builder.MapGroup(BaseUrl).HasApiVersion(1);

        chat.MapPost("groups/{groupId}/messages", CreateMessage).WithSummary("Creates a new message");
        chat.MapGet("messages", GetGroupMessages).WithSummary("Gets all messages");


        return builder;
    }

    private static async Task<IResult> CreateMessage(ISender sender, IClaimsService claimsService, [Required] string groupId, [FromBody] MessageCreateDto dto)
    {
        var userId = claimsService.GetCurrentUserId;
        var result = await sender.Send(new CreateMessageCommand{GroupId = groupId, UserId = userId, Message = dto});
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetGroupMessages(ISender sender, IClaimsService claimsService, [FromQuery] string groupId, [AsParameters] QueryFilter filter)
    {
        // var userId = claimsService.GetCurrentUserId;
        var userId = "b93d6316-be4c-4885-a5e0-eae1ea3d1379";
        var result = await sender.Send(new GetGroupMessageByIdQuery() {GroupId = groupId, UserId = userId, Filter = filter});
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    } 
    
    private static IResult HandlerFailure(Result result) =>
        result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException(),
            IValidationResult validationResult =>
                Results.BadRequest(
                    CreateProblemDetails(
                        "Validation Error", StatusCodes.Status400BadRequest,
                        result.Error,
                        validationResult.Errors)),
            _ =>
                Results.BadRequest(
                    CreateProblemDetails(
                        "Bab Request", StatusCodes.Status400BadRequest,
                        result.Error))
        };

    private static ProblemDetails CreateProblemDetails(
        string title,
        int status,
        Error error,
        Error[]? errors = null) =>
        new()
        {
            Title = title,
            Type = error.Code,
            Detail = error.Message,
            Status = status,
            Extensions = { { nameof(errors), errors } }
        };
}