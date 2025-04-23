using Asp.Versioning.Builder;
using ChatService.Contract.Abstractions.Shared;
using ChatService.Contract.DTOs.GroupDtos;
using ChatService.Contract.Services.Group;
using ChatService.Contract.Services.Group.Commands;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace ChatService.Presentation.V1;

public static class GroupEndpoints
{
    public const string ApiName = "groups";
    private const string BaseUrl = $"/api/v{{version:apiVersion}}/{ApiName}";

    public static IVersionedEndpointRouteBuilder MapGroupApiV1(this IVersionedEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup(BaseUrl).HasApiVersion(1);

        group.MapPost("", CreateGroup);
        group.MapPatch("{groupId}", UpdateGroup);
        group.MapPost("{groupId}/members/{userId}", PromoteGroupMember);

        return builder;
    }

    private static async Task<IResult> CreateGroup(ISender sender, [FromBody] CreateGroupCommand request)
    {
        var result = await sender.Send(request);
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> UpdateGroup(ISender sender, string groupId, [FromBody] GroupUpdateDto dto)
    {
        var result = await sender.Send(new UpdateGroupCommand {GroupId = groupId, GroupUpdateDto = dto});
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> PromoteGroupMember(ISender sender, string groupId, string userId, [FromBody] GroupUpdateDto dto)
    {
        var result = await sender.Send(new PromoteGroupMemberCommand {GroupId = groupId, MemberId = userId});
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